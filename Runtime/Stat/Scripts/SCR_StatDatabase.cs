using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public static class StatDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static StatDefinition[] database = Array.Empty<StatDefinition>();

        internal static void Build(string[] idCollection, StatEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);
            database = new StatDefinition[entries.Length];

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];

                idLookup[key] = i;
                idSearch.Entries[i] = new(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[i] = new(entries[i]);
            }

            Debug.Log($"Stat database build successfull!");
        }

        public static IReadOnlyList<StatDefinition> GetDatabase() => database;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static StatDefinition GetDefinition(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Stat not found index out of range");
            }

            return database[index];
        }
        public static StatDefinition GetDefinition(StatID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Stat id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
    }
}