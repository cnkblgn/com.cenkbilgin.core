using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MeshDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static Mesh[] meshes = Array.Empty<Mesh>();

        internal static void Build(Mesh[] _meshes)
        {
            if (_meshes == null)
            {
                return;
            }

            List<Mesh> validMeshes = new(_meshes.Length);
            idLookup.Clear();

            for (int i = 0; i < _meshes.Length; i++)
            {
                Mesh mesh = _meshes[i];

                if (mesh == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Mesh database mesh is invalid at index {i}!");
#endif
                    continue;
                }

                string key = mesh.name;

                idLookup[key] = validMeshes.Count;
                validMeshes.Add(mesh);
            }

            meshes = validMeshes.ToArray();
            version++;

            Debug.Log($"Mesh database build successful! Mesh count: {meshes.Length}");
        }

        public static int GetMeshes() => meshes.Length;
        public static int GetIDIndex(string key) => key != null && idLookup.TryGetValue(key, out int index) ? index : -1;
        public static MeshID GetID(int index)
        {
            if (index >= meshes.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Mesh id not found index out of range");
            }

            return new(meshes[index].name, index);
        }
        public static Mesh GetMesh(int index)
        {
            if (index >= meshes.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Mesh not found index out of range");
            }

            return meshes[index];
        }
        public static Mesh GetMesh(MeshID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Mesh id [{id}] is not valid!");
            }

            return GetMesh(id.Index);
        }
    }
}