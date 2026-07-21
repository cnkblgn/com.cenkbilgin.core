using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public static class StatDatabase
    {
        private static readonly Dictionary<StatID, StatDefinition> database = new();
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());

        internal static void Build(StatEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            database.Clear();
            ids = new SearchCollection<string>(new SearchEntry<string>[entries.Length]);

            for (int i = 0; i < entries.Length; i++)
            {
                StatEntry entry = entries[i];
                string key = entry.Key;

                StatID id = new(key, i);

                database[id] = new(id, entry.Default, entry.Min, entry.Max);
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Stat database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static int GetIndex(string id) => GetDefinition(new(id, -1)).ID.Index;
        public static StatID GetID(int index)
        {
            int i = 0;

            foreach (StatID id in database.Keys)
            {
                if (index == i)
                {
                    return id;
                }

                i++;
            }

            return default;
        }
        public static IReadOnlyCollection<StatDefinition> GetDatabase() => database.Values;
        public static StatDefinition GetDefinition(StatID id)
        {
            if (!database.TryGetValue(id, out StatDefinition definition))
            {
                throw new ArgumentNullException($"undefined stat id [{id.Key}]");
            }

            return definition;
        }
    }
}