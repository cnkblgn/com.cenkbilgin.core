using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        private static SearchCollection<string> tags = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> tagDatabase = new();

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

                tagDatabase[key] = index;
                tags.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Damage database build successfull!");
        }

        public static SearchCollection<string> GetTags() => tags;
        public static int GetIndex(string id)
        {
            if (tagDatabase.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
    }
}