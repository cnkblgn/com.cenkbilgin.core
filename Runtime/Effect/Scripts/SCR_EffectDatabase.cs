using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public static class EffectDatabase
    {
        private static readonly Dictionary<EffectID, EffectDefinition> database = new();
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());

        internal static void Build(EffectEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            database.Clear();
            ids = new SearchCollection<string>(new SearchEntry<string>[entries.Length]);

            for (int i = 0; i < entries.Length; i++)
            {
                EffectEntry entry = entries[i];
                string key = entry.Key;

                EffectID id = new(key, i);

                database[id] = new(id, entry.IconID, entry.NameID, entry.Action, entry.Tag, entry.Interval);
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Effect database build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static EffectID GetID(int index)
        {
            int i = 0;

            foreach (EffectID id in database.Keys)
            {
                if (index == i)
                {
                    return id;
                }

                i++;
            }

            return default;
        }
        public static int GetIndex(string id) => GetDefinition(new EffectID(id, -1)).ID.Index;

        public static IReadOnlyCollection<EffectDefinition> GetDefinitions() => database.Values;
        public static EffectDefinition GetDefinition(EffectID id)
        {
            if (!database.TryGetValue(id, out EffectDefinition definition))
            {
                throw new ArgumentNullException($"undefined effect id [{id.Key}]");
            }

            return definition;
        }
        internal static EffectInstance CreateInstance(EffectID id, float duration)
        {
            if (!database.ContainsKey(id))
            {
                throw new ArgumentNullException($"effect definition not found for [{id}]");
            }

            return new(id, duration);
        }
    }
}