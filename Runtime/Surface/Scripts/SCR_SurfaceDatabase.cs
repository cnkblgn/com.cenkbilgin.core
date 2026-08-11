using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static readonly Dictionary<string, int> tagLookup = new();
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static SurfaceTag[] database = Array.Empty<SurfaceTag>();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            tagLookup.Clear();
            tagSearch = new SearchCollection<string>(new SearchEntry<string>[tagCollection.Length + 1]);
            database = new SurfaceTag[tagCollection.Length + 1];

            database[0] = new("GENERIC", 0);
            tagSearch.Entries[0] = new("GENERIC", "GENERIC");

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                tagLookup[key] = index;
                database[index] = new(key, index);
                tagSearch.Entries[index] = new(key, key);
            }

            Debug.Log($"Surface database build successfull!");
        }

        public static SearchCollection<string> GetTags() => tagSearch;
        public static SurfaceTag GetTag(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Surface  tag not found index out of range");
            }

            return database[index];
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}
