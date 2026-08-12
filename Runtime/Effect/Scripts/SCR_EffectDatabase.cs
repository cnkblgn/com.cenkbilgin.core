using Core.Actors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public static class EffectDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static EffectDefinition[] database = Array.Empty<EffectDefinition>();

        internal static void Build(string[] idCollection, EffectEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);
            database = new EffectDefinition[entries.Length];

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i;

                idLookup[key] = index;
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[i] = new(entries[i]);
            }

            Debug.Log($"Effect database build successfull!");
        }

        public static IReadOnlyList<EffectDefinition> GetDatabase() => database;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static EffectDefinition GetDefinition(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Effect database index out of range");
            }

            return database[index];
        }
        public static EffectDefinition GetDefinition(EffectID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Effect id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
        public static EffectInstance CreateInstance(EffectID id, float duration) => new(GetDefinition(id), duration);
    }
}