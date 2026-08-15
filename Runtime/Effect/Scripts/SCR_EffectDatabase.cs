using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public static class EffectDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static EffectDefinition[] definitions = Array.Empty<EffectDefinition>();

        internal static void Build(EffectEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new EffectDefinition[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                string key = entries[i].ID.Key;
                int index = i;

                idLookup[key] = index;
                definitions[i] = new(entries[i]);
            }

            Debug.Log($"Effect database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<EffectDefinition> GetDefinitions() => definitions;
        public static EffectDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Effect database index out of range");
            }

            return definitions[index];
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