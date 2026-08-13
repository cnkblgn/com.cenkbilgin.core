using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class MeshDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static Mesh[] database = Array.Empty<Mesh>();

        internal static void Build(Mesh[] meshCollection)
        {
            if (meshCollection == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new SearchCollection<string>(new SearchEntry<string>[meshCollection.Length]);
            database = new Mesh[meshCollection.Length];

            for (int i = 0; i < meshCollection.Length; i++)
            {
                Mesh obj = meshCollection[i];

#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug.LogError("Mesh database object is null!");
                    continue;
                }
#endif

                string key = obj.name;

                idLookup[key] = i;
                idSearch.Entries[i] = new(key, key);
                database[i] = obj;
            }

            Debug.Log($"Mesh database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => idSearch;
        public static MeshID GetID(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Mesh id not found index out of range");
            }

            return new(idSearch.Entries[index].Value, index);
        }
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static Mesh GetMesh(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Mesh not found index out of range");
            }

            return database[index];
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