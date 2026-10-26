using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MaterialDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static Material[] materials = Array.Empty<Material>();

        internal static void Build(Material[] _materials)
        {
            if (_materials == null)
            {
                return;
            }

            List<Material> validMaterials = new(_materials.Length);
            idLookup.Clear();

            for (int i = 0; i < _materials.Length; i++)
            {
                Material material = _materials[i];

                if (material == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Material database material is invalid at index {i}!");
#endif
                    continue;
                }

                string key = material.name;

                idLookup[key] = validMaterials.Count;
                validMaterials.Add(material);
            }

            materials = validMaterials.ToArray();
            version++;

            Debug.Log($"Material database build successful! Material count: {materials.Length}");
        }

        public static int GetMaterials() => materials.Length;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static MaterialID GetID(int index)
        {
            if (index >= materials.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Material id not found index out of range");
            }

            return new(materials[index].name, index);
        }
        public static Material GetMaterial(int index)
        {
            if (index >= materials.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Material not found index out of range");
            }

            return materials[index];
        }
        public static Material GetMaterial(MaterialID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Material id [{id}] is not valid!");
            }

            return GetMaterial(id.Index);
        }
    }
}