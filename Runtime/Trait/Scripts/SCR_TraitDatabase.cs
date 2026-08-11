using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    public static class TraitDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static TraitDefinition[] database = Array.Empty<TraitDefinition>();

        internal static void Build(string[] idCollection, TraitEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);
            database = new TraitDefinition[idCollection.Length];

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

            Debug.Log($"Trait database build successfull!");
        }

        public static IReadOnlyList<TraitDefinition> GetDatabase() => database;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static TraitDefinition GetDefinition(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Trait not found index out of range");
            }

            return database[index];
        }
        public static TraitDefinition GetDefinition(TraitID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Trait id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
        public static TraitInstance CreateInstance(TraitID id) => new(GetDefinition(id).ID);
    }
}