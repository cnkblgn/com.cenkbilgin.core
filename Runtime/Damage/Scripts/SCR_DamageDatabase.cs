using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        private static SearchCollection<string> tagSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, DamageTag> database = new();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            database.Clear();
            tagSearch = new(new SearchEntry<string>[tagCollection.Length]);

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                DamageTag tag = new(key, index);

                database[key] = tag;
                tagSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Damage database build successfull!");
        }

        public static SearchCollection<string> GetTags() => tagSearch;
        public static string GetTagKey(int index)
        {
            if (index < 0 || index >= database.Count)
            {
                throw new IndexOutOfRangeException($"DamageTag index [{index}] is not valid!");
            }

            return tagSearch.Entries[index].Value;
        }
        public static int GetTagIndex(string key)
        {
            if (database.TryGetValue(key, out DamageTag tag))
            {
                return tag.Index;
            }

            return -1;
        }
    }
}