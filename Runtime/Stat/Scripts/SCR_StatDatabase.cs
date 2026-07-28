using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public static class StatDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<StatID, StatDefinition> database = new();

        internal static void Build(string[] idCollection, StatEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            database.Clear();

            idSearch = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];

                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[entries[i].ID] = new(entries[i]);
            }

            Debug.Log($"Effect database build successfull!");
        }

        public static IReadOnlyCollection<StatDefinition> GetDatabase() => database.Values;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static StatDefinition GetDefinition(StatID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Stat id [{nameof(id)}] is not valid!");
            }

            if (!database.TryGetValue(id, out StatDefinition definition))
            {
                throw new ArgumentNullException($"undefined stat id [{id.Key}]");
            }

            return definition;
        }
    }
}