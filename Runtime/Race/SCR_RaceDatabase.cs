using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Race
{
    public static class RaceDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static readonly Dictionary<string, int> tagLookup = new();
        private static RaceDefinition[] definitions = Array.Empty<RaceDefinition>();
        private static RaceTag[] tags = Array.Empty<RaceTag>();

        internal static void Build(RaceEntry[] entries, string[] _tags)
        {
            if (entries == null || _tags == null)
            {
                return;
            }

            if (_tags.Length > 63)
            {
                throw new InvalidOperationException("Race database supports a maximum of 63 non-generic tags.");
            }

            idLookup.Clear();
            tagLookup.Clear();

            tags = new RaceTag[_tags.Length];
            definitions = new RaceDefinition[entries.Length];

            for (int i = 0; i < _tags.Length; i++)
            {
                int index = i;
                string key = _tags[i];

                tagLookup[key] = index;
                tags[index] = new RaceTag(key, index);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                RaceEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Race database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<RaceTag> GetTags() => tags;
        public static IReadOnlyList<RaceDefinition> GetDefinitions() => definitions;
        public static RaceDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Race not found index out of range");
            }

            return definitions[index];
        }
        public static RaceDefinition GetDefinition(RaceID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Race id [{id}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
    }
}