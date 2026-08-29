using Core.Actors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Quest
{
    public static class QuestDatabase
    {
        public static event Action<QuestID> OnQuestProgress;
        public static event Action<QuestID> OnQuestStarted;
        public static event Action<QuestID> OnQuestCompleted;

        private static readonly Dictionary<string, int> idLookup = new();
        private static QuestDefinition[] definitions = Array.Empty<QuestDefinition>();
        private static List<QuestInstance> quests = new();

        internal static void Build(QuestEntry[] entries)
        {
            OnQuestProgress = null;
            OnQuestStarted = null;
            OnQuestCompleted = null;
            quests = new();

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

        public static List<QuestInstance> Export() => new(quests);
        public static void Import(List<QuestInstance> data)
        {
            data.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                data.Add(data[i]);
            }
        }

        public static bool TryStart(QuestID id)
        {
            if (!id.IsValid)
            {
                return false;
            }

            foreach (QuestInstance quest in quests)
            {
                if (quest.Definition.ID != id)
                {
                    continue;
                }

                return false;
            }

            QuestInstance instance = id.CreateInstance();
            quests.Add(instance);

            HandleQuestStarted(id);

            TryComplete(instance);
            return true;
        }
        public static bool TryComplete(QuestID id)
        {
            QuestInstance quest = GetInstance(id);

            if (quest == null)
            {
                return false;
            }

            return TryComplete(quest);
        }
        private static bool TryComplete(QuestInstance quest)
        {
            if (quest.IsCompleted || !quest.CanComplete())
            {
                return false;
            }

            quest.IsCompleted = true;
            HandleQuestCompleted(quest.Definition.ID);
            return true;
        }
        
        public static bool IsActive(QuestID id) => !GetInstance(id).CanComplete();
        public static bool IsCompleted(QuestID id) => GetInstance(id).CanComplete();

        private static void HandleQuestStarted(QuestID id)
        {
            OnQuestStarted?.Invoke(id);

            QuestAction[] actions = id.GetDefinition().Actions;

            foreach (QuestAction action in actions)
            {
                action.Started();
            }
        }
        private static void HandleQuestCompleted(QuestID id)
        {
            OnQuestCompleted?.Invoke(id);

            QuestAction[] actions = id.GetDefinition().Actions;

            foreach (QuestAction action in actions)
            {
                action.Completed();
            }

            QuestDefinition definition = id.GetDefinition();

            if (definition.NextID.IsValid)
            {
                TryStart(definition.NextID);
            }
        }
        private static void HandleQuestProgressed(QuestID id)
        {
            OnQuestProgress?.Invoke(id);
        }

        public static void Notify(QuestEvent @event, ActorID id, ulong tags, int amount = 1)
        {
            int count = quests.Count;

            for (int i = 0; i < count; i++)
            {
                QuestInstance quest = quests[i];

                if (quest.IsCompleted)
                {
                    continue;
                }

                if (quest.Notify(@event, id, tags, amount))
                {
                    HandleQuestProgressed(quest.Definition.ID);
                }

                TryComplete(quest);
            }
        }
        public static void Notify(QuestEvent @event, ActorID id, ActorTag tag, int amount = 1) => Notify(@event, id, tag.Mask, amount);
        public static void Notify(QuestEvent @event, ActorID id, ActorTag[] tags, int amount = 1) => Notify(@event, id, tags.CreateMask(), amount);
        public static void Notify(QuestEvent @event, ActorID id, int amount = 1) => Notify(@event, id, 0, amount);
        public static void Notify(QuestEvent @event, ActorID id) => Notify(@event, id, 0, 1);

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
        public static QuestInstance GetInstance(QuestID id)
        {
            foreach (QuestInstance quest in quests)
            {
                if (quest.Definition.ID == id)
                {
                    return quest;
                }
            }

            return null;
        }
        public static QuestInstance CreateInstance(QuestID id) => new(GetDefinition(id));
    }
}