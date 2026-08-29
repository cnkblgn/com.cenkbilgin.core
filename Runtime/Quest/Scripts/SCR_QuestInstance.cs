using System;
using Core.Actors;

namespace Core.Quest
{
    [Serializable]
    public sealed class QuestInstance
    {
        public readonly QuestDefinition Definition;
        public readonly int[] Progress;
        public bool IsCompleted;

        public QuestInstance(QuestDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Progress = new int[definition.Requirements.Length];
        }
        internal QuestInstance(QuestID id, int[] progress, bool isCompleted)
        {
            Definition = !id.IsValid ? throw new ArgumentNullException(nameof(id)) : id.GetDefinition();
            Progress = (int[])progress.Clone();
            IsCompleted = isCompleted;
        }

        public bool CanComplete()
        {
            if (IsCompleted)
            {
                return true;
            }

            for (int i = 0; i < Definition.Requirements.Length; i++)
            {
                if (Progress[i] < Definition.Requirements[i].Amount)
                {
                    return false;
                }
            }

            for (int i = 0; i < Definition.Conditions.Length; i++)
            {
                if (!Definition.Conditions[i].IsSatisfied())
                {
                    return false;
                }
            }

            return true;
        }
        public bool Notify(QuestEvent @event, ActorID id, ulong tags, int amount)
        {
            bool hasChanged = false;

            for (int i = 0; i < Definition.Requirements.Length; i++)
            {
                QuestRequirement requirement = Definition.Requirements[i];

                if (!requirement.IsMatch(@event, id, tags))
                {
                    continue;
                }

                int previous = Progress[i];
                Progress[i] = Math.Min(previous + amount, requirement.Amount);

                if (previous != Progress[i])
                {
                    hasChanged = true;
                }
            }

            return hasChanged;
        }
    }
}