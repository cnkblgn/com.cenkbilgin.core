using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        private static SearchCollection<string> tags = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, DamageTag> tagDatabase = new();

        internal static void Build(string[] entries)
        {
            if (entries == null)
            {
                return;
            }

            tagDatabase.Clear();
            tags = new(new SearchEntry<string>[entries.Length]);

            for (int i = 0; i < entries.Length; i++)
            {
                string key = entries[i];
                int index = i + 1;

                DamageTag tag = new(key, index);

                tagDatabase[key] = tag;
                tags.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Damage database build successfull!");
        }
        public static SearchCollection<string> GetTags() => tags;
        public static int GetIndex(string key)
        {
            if (tagDatabase.TryGetValue(key, out DamageTag tag))
            {
                return tag.Index;
            }

            return -1;
        }
    }
}