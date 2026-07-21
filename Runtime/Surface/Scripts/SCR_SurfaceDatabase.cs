using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static readonly Dictionary<string, int> database = new();
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());

        internal static void Build(string[] collection)
        {
            if (collection == null)
            {
                return;
            }

            database.Clear();

            ids = new SearchCollection<string>(new SearchEntry<string>[collection.Length]);

            for (int i = 0; i < collection.Length; i++)
            {
                string key = collection[i];
                int index = i + 1;

                database[key] = index;
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Surface database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static int GetIndex(string id)
        {
            if (database.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
    }
}
