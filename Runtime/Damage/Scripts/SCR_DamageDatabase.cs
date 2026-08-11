using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        private static readonly Dictionary<string, int> tagLookup = new();
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static DamageTag[] database = Array.Empty<DamageTag>();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            database = new DamageTag[tagCollection.Length + 1];
            tagSearch = new(new SearchEntry<string>[tagCollection.Length + 1]);
            tagLookup.Clear();

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

            Debug.Log($"Damage database build successfull!");
        }

        public static SearchCollection<string> GetTags() => tagSearch;
        public static DamageTag GetTag(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Damage tag not found index out of range");
            }

            return database[index];
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}