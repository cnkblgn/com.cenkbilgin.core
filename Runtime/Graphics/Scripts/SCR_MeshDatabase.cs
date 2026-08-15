using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MeshDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static Mesh[] meshes = Array.Empty<Mesh>();

        internal static void Build(Mesh[] meshCollection)
        {
            if (meshCollection == null)
            {
                return;
            }

            idLookup.Clear();
            meshes = new Mesh[meshCollection.Length];

            for (int i = 0; i < meshCollection.Length; i++)
            {
                Mesh mesh = meshCollection[i];
#if UNITY_EDITOR
                if (mesh == null)
                {
                    Debug.LogError("Mesh database mesh is invalid!");
                    continue;
                }
#endif
                string key = mesh.name;

                idLookup[key] = i;
                meshes[i] = mesh;
            }

            Debug.Log($"Mesh database build successfull!");
        }

        public static int GetMeshes() => meshes.Length;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
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
                throw new ArgumentNullException($"[{nameof(id)}] MeshID is not valid!");
            }

            return GetMesh(id.Index);
        }
    }
}