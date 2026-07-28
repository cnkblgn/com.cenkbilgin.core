using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> database = new();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            database.Clear();

            tagSearch = new SearchCollection<string>(new SearchEntry<string>[tagCollection.Length]);

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                database[key] = index;
                tagSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Surface database build successfull!");
        }

        public static SearchCollection<string> GetTags() => tagSearch;
        public static string GetTagKey(int index)
        {
            if (index < 0 || index >= database.Count)
            {
                throw new IndexOutOfRangeException($"SurfaceTag index [{index}] is not valid!");
            }

            return tagSearch.Entries[index].Value;
        }
        public static int GetTagIndex(string id)
        {
            if (database.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
    }
}
