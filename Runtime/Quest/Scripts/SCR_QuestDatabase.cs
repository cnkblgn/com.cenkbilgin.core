using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Core.Actors;

namespace Core.Quest
{
    public static class QuestDatabase
    {
        public static event Action<QuestID> OnQuestProgress;
        public static event Action<QuestID> OnQuestStarted;
        public static event Action<QuestID> OnQuestCompleted;

        private static readonly Dictionary<string, int> idLookup = new();
        private static QuestDefinition[] definitions = Array.Empty<QuestDefinition>();
        private static List<QuestInstance> instances = new();

        internal static void Build(QuestEntry[] entries)
        {
            OnQuestProgress = null;
            OnQuestStarted = null;
            OnQuestCompleted = null;
            instances = new();

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

        public static List<QuestInstance> Export() => new(instances);
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

            if (GetInstance(id) != null)
            {
                return false;
            }

            QuestInstance instance = id.CreateInstance();
            instances.Add(instance);

            instance.Start();

            HandleQuestStarted(id);

            TryComplete(instance);
            return true;
        }
        public static bool TryComplete(QuestID id)
        {
            QuestInstance instance = GetInstance(id);

            if (instance == null)
            {
                return false;
            }

            return TryComplete(instance);
        }
        private static bool TryComplete(QuestInstance quest)
        {
            if (quest.IsCompleted || !quest.CanComplete())
            {
                return false;
            }

            quest.Complete();

            HandleQuestCompleted(quest.Definition.ID);
            return true;
        }
        internal static void NotifyProgress(QuestInstance instance)
        {
            OnQuestProgress?.Invoke(instance.Definition.ID);
            TryComplete(instance);
        }

        public static bool IsActive(QuestID id)
        {
            QuestInstance quest = GetInstance(id);
            return quest != null && !quest.IsCompleted;
        }
        public static bool IsCompleted(QuestID id)
        {
            QuestInstance quest = GetInstance(id);
            return quest != null && quest.IsCompleted;
        }

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
            foreach (QuestInstance quest in instances)
            {
                if (quest.Definition.ID == id)
                {
                    return quest;
                }
            }

            return null;
        }
        public static QuestInstance CreateInstance(QuestID id) => new(GetDefinition(id));

#if UNITY_EDITOR
        public static void LogAll()
        {
            for (int i = 0; i < instances.Count; i++)
            {
                Log(instances[i]);
            }
        }
        public static void Log(QuestID id) => Log(GetInstance(id));
        private static void Log(QuestInstance instance)
        {
            if (instance == null)
            {
                Debug.LogWarning($"Quest log failed! instance is null!");
                return;
            }

            QuestRequirement[] requirements = instance.Definition.Requirements;
            QuestCondition[] conditions = instance.Definition.Conditions;

            StringBuilder sb = new();
            sb.AppendLine($"ID {instance.Definition.ID.Key}");
            sb.AppendLine($"  Completed: {instance.IsCompleted}");

            if (requirements.Length > 0)
            {
                sb.AppendLine("  Requirements:");

                for (int i = 0; i < requirements.Length; i++)
                {
                    QuestRequirement requirement = requirements[i];

                    sb.Append("    [");
                    sb.Append(i);
                    sb.Append("] ");
                    sb.Append(requirement.Event.Key);
                    sb.Append(" | ");
                    sb.Append(instance.Progress[i]);
                    sb.Append(" / ");
                    sb.Append(requirement.Amount);

                    if (requirement.Actor.IsValid)
                    {
                        sb.Append(" | Actor: ");
                        sb.Append(requirement.Actor.Key);
                    }

                    ulong tagMask = requirement.Tags.CreateMask();

                    if (tagMask != 0)
                    {
                        sb.Append(" | Tags: ");
                        sb.Append(tagMask);
                    }

                    if (instance.Progress[i] >= requirement.Amount)
                    {
                        sb.Append(" ✓");
                    }
                    else
                    {
                        sb.Append(" ...");
                    }

                    sb.AppendLine();
                }
            }

            if (conditions.Length > 0)
            {
                sb.AppendLine("  Conditions:");

                for (int i = 0; i < conditions.Length; i++)
                {
                    bool satisfied = conditions[i].IsSatisfied();

                    sb.Append("    [");
                    sb.Append(i);
                    sb.Append("] ");
                    sb.Append(satisfied ? "✓ Satisfied" : "... Not satisfied");
                    sb.AppendLine();
                }
            }

            sb.Append("  Can Complete: ");
            sb.Append(instance.CanComplete());

            Debug.Log(sb.ToString());
        }
#endif
    }
}