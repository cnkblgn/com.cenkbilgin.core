using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Quest
{
    public static class QuestDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static QuestDefinition[] database = Array.Empty<QuestDefinition>();

        internal static void Build(string[] idCollection, QuestEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new(new SearchEntry<string>[idCollection.Length]);
            database = new QuestDefinition[entries.Length];

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];

                idLookup[key] = i;
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[i] = new(entries[i]);
            }

            Debug.Log($"Quest database build successfull!");
        }

        public static IReadOnlyList<QuestDefinition> GetDatabase() => database;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static QuestDefinition GetDefinition(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Quest not found index out of range");
            }

            return database[index];
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