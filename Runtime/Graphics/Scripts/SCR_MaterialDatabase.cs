using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MaterialDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static Material[] materials = Array.Empty<Material>();

        internal static void Build(Material[] materialCollection)
        {
            if (materialCollection == null)
            {
                return;
            }

            idLookup.Clear();
            materials = new Material[materialCollection.Length];

            for (int i = 0; i < materialCollection.Length; i++)
            {
                Material material = materialCollection[i];

#if UNITY_EDITOR
                if (material == null)
                {
                    Debug.LogError("Material database material is invalid!");
                    continue;
                }
#endif

                string key = material.name;

                idLookup[key] = i;
                materials[i] = material;
            }

            Debug.Log($"Material database build successfull!");
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
                throw new ArgumentNullException($"[{nameof(id)}] MaterialID is not valid!");
            }

            return GetMaterial(id.Index);
        }
    }
}