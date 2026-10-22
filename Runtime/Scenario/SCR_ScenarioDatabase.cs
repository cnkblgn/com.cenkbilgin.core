using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Scenario
{
    public static class ScenarioDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static ScenarioDefinition[] definitions = Array.Empty<ScenarioDefinition>();

        internal static void Build(ScenarioEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new ScenarioDefinition[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                ScenarioEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Scenario database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<ScenarioDefinition> GetDefinitions() => definitions;
        public static ScenarioDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Scenario not found index out of range");
            }

            return definitions[index];
        }
        public static ScenarioDefinition GetDefinition(ScenarioID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Scenario id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
    }
}