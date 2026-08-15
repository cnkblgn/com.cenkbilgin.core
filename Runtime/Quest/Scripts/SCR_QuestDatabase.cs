using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Quest
{
    public static class QuestDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static QuestDefinition[] definitions = Array.Empty<QuestDefinition>();

        internal static void Build(QuestEntry[] entries)
        {
            if (entries == null)
            {
                return;
            }

            idLookup.Clear();
            definitions = new QuestDefinition[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                QuestEntry entry = entries[i];
                string key = entry.ID.Key;

                idLookup[key] = i;
                definitions[i] = new(entry);
            }

            Debug.Log($"Quest database build successfull!");
        }

        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IReadOnlyList<QuestDefinition> GetDefinitions() => definitions;
        public static QuestDefinition GetDefinition(int index)
        {
            if (index >= definitions.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Quest not found index out of range");
            }

            return definitions[index];
        }
        public static QuestDefinition GetDefinition(QuestID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Quest id [{nameof(id)}] is not valid!");
            }

            return GetDefinition(id.Index);
        }
        public static QuestInstance CreateInstance(QuestID id) => new(GetDefinition(id));
    }
}