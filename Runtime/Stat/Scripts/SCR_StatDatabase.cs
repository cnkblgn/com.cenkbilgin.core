using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Stat
{
    public static class StatDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static StatDefinition[] definitions = Array.Empty<StatDefinition>();

        internal static void Build(StatEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new StatDefinition[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                StatEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Stat database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<StatDefinition> GetDefinitions() => definitions;
        public static StatDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Stat not found index out of range");
            }

            return definitions[index];
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