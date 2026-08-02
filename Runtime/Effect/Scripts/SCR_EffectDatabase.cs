using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public static class EffectDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<EffectID, EffectDefinition> database = new();

        internal static void Build(string[] idCollection, EffectEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            database.Clear();
            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];
                int index = i;

                idLookup[key] = index;
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[entries[i].ID] = new(entries[i]);
            }

            Debug.Log($"Effect database build successfull!");
        }

        public static IReadOnlyCollection<EffectDefinition> GetDatabase() => database.Values;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static EffectID GetID(int index)
        {
            if (index < 0 || index >= idSearch.Entries.Length)
            {
                throw new ArgumentOutOfRangeException("Effect get id failed! index out of bounds!");
            }

            return new(idSearch.Entries[index].Value, index);
        }
        public static int GetIDIndex(string id)
        {
            if (idLookup.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        public static EffectDefinition GetDefinition(EffectID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Effect id [{nameof(id)}] is not valid!");
            }

            if (!database.TryGetValue(id, out EffectDefinition definition))
            {
                throw new ArgumentNullException($"undefined effect id [{id.Key}]");
            }

            return definition;
        }
        public static EffectInstance CreateInstance(EffectID id, float duration)
        {
            if (!database.ContainsKey(id))
            {
                throw new ArgumentNullException($"effect definition not found for [{id}]");
            }

            return new(id, duration);
        }
    }
}