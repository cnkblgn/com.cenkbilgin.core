using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Quest
{
    public static class QuestDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<QuestID, QuestDefinition> database = new();

        internal static void Build(string[] idCollection, QuestEntry[] entries)
        {
            if (idCollection == null || entries == null)
            {
                return;
            }

            database.Clear();

            idSearch = new(new SearchEntry<string>[idCollection.Length]);

            for (int i = 0; i < idCollection.Length; i++)
            {
                string key = idCollection[i];

                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            for (int i = 0; i < entries.Length; i++)
            {
                database[entries[i].ID] = new(entries[i]);
            }

            Debug.Log($"Quest database build successfull!");
        }

        public static IReadOnlyCollection<QuestDefinition> GetDatabase() => database.Values;
        public static SearchCollection<string> GetIDs() => idSearch;
        public static QuestDefinition GetDefinition(QuestID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"Quest id [{nameof(id)}] is not valid!");
            }

            if (!database.TryGetValue(id, out QuestDefinition definition))
            {
                throw new NullReferenceException($"quest definition not found for [{id}] please check quest database!");
            }

            return definition;
        }
        public static QuestInstance CreateInstance(QuestID id) => new(GetDefinition(id));
    }
}