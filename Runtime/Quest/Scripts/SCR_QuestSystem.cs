using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Actors;

namespace Core.Quest
{
    public static class QuestSystem
    {
        public static event Action<QuestID> OnQuestProgress;
        public static event Action<QuestID> OnQuestStarted;
        public static event Action<QuestID> OnQuestCompleted;

        private static readonly List<QuestInstance> quests = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize()
        {
            quests.Clear();
            OnQuestProgress = null;
            OnQuestStarted = null;
            OnQuestCompleted = null;
        }

        public static List<QuestInstance> Export() => new(quests);
        public static void Import(List<QuestInstance> progress)
        {
            progress.Clear();

            for (int i = 0; i < progress.Count; i++)
            {
                progress.Add(progress[i]);
            }
        }

        public static bool TryStartQuest(QuestID id)
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

            TryCompleteQuest(instance);
            return true;
        }
        public static bool TryCompleteQuest(QuestID id)
        {
            QuestInstance quest = GetQuest(id);

            if (quest == null)
            {
                return false;
            }

            return TryCompleteQuest(quest);
        }
        private static bool TryCompleteQuest(QuestInstance quest)
        {
            if (quest.IsCompleted || !quest.CanComplete())
            {
                return false;
            }

            quest.IsCompleted = true;
            HandleQuestCompleted(quest.Definition.ID);
            return true;
        }

        public static QuestInstance GetQuest(QuestID id)
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

        public static bool IsActive(QuestID id) => !GetQuest(id).CanComplete();
        public static bool IsCompleted(QuestID id) => GetQuest(id).CanComplete();

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
                TryStartQuest(definition.NextID);
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

                TryCompleteQuest(quest);
            }
        }
        public static void Notify(QuestEvent @event, ActorID id, ActorTag tag, int amount = 1) => Notify(@event, id, tag.Mask, amount);
        public static void Notify(QuestEvent @event, ActorID id, ActorTag[] tags, int amount = 1) => Notify(@event, id, tags.CreateMask(), amount);
        public static void Notify(QuestEvent @event, ActorID id, int amount = 1) => Notify(@event, id, 0, amount);
        public static void Notify(QuestEvent @event, ActorID id) => Notify(@event, id, 0, 1);
    }
}