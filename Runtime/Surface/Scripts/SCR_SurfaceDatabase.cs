using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, SurfaceTag> database = new();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            database.Clear();
            tagSearch = new SearchCollection<string>(new SearchEntry<string>[tagCollection.Length + 1]);

            database["GENERIC"] = new("GENERIC", 0);
            tagSearch.Entries[0] = new("GENERIC", "GENERIC");

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                database[key] = new(key, index);
                tagSearch.Entries[index] = new SearchEntry<string>(key, key);
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
            if (database.TryGetValue(id, out SurfaceTag tag))
            {
                return tag.Index;
            }

            return -1;
        }
    }
}
