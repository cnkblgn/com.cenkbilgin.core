using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    public static class TraitDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static TraitDefinition[] definitions = Array.Empty<TraitDefinition>();

        internal static void Build(TraitEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new TraitDefinition[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                TraitEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            version++;
            Debug.Log($"Trait database build successfull!");
        }

        public static int GetIDIndex(string key) => key != null && idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<TraitDefinition> GetDefinitions() => definitions;
        public static TraitDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Trait not found index out of range");
            }

            return definitions[index];
        }
        public static TraitDefinition GetDefinition(TraitID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Trait id [{id}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
    }
}