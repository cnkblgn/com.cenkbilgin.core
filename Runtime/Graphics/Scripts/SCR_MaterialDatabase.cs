using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MaterialDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static Material[] database = Array.Empty<Material>();

        internal static void Build(Material[] materialCollection)
        {
            if (materialCollection == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new SearchCollection<string>(new SearchEntry<string>[materialCollection.Length]);
            database = new Material[materialCollection.Length];

            for (int i = 0; i < materialCollection.Length; i++)
            {
                Material obj = materialCollection[i];

#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug.LogError("Material database object is null!");
                    continue;
                }
#endif

                string key = obj.name;

                idLookup[key] = i;
                idSearch.Entries[i] = new(key, key);
                database[i] = obj;
            }

            Debug.Log($"Material build successfull!");
        }

        public static SearchCollection<string> GetIDs() => idSearch;
        public static MaterialID GetID(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Material id not found index out of range");
            }

            return new(idSearch.Entries[index].Value, index);
        }
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static Material GetMaterial(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Material not found index out of range");
            }

            return database[index];
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