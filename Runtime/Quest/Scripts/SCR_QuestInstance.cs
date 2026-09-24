using System;
using Core.Event;

namespace Core.Quest
{
    [Serializable]
    public sealed class QuestInstance
    {
        public readonly QuestDefinition Definition;
        public readonly int[] Progress;
        public bool IsCompleted;

        [NonSerialized] private bool isStarted;
        [NonSerialized] private Action<EventContext> callback;

        public QuestInstance(QuestDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition), "Quest instace ctor failed! definition is null!?");
            Progress = new int[definition.Requirements.Length];
        }
        internal QuestInstance(QuestID id, int[] progress, bool isCompleted)
        {
            Definition = !id.IsValid ? throw new ArgumentNullException(nameof(id), "Quest instace ctor failed! id is not valid!?") : id.GetDefinition();
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
        internal void Start()
        {
            if (IsCompleted || isStarted)
            {
                return;
            }

            isStarted = true;
            callback = OnEvent;

            QuestRequirement[] requirements = Definition.Requirements;

            for (int i = 0; i < requirements.Length; i++)
            {
                EventID id = requirements[i].Event;

                if (!id.IsValid)
                {
                    continue;
                }

                EventDatabase.Subscribe(id, callback);
            }
        }
        internal void Complete()
        {
            if (callback == null)
            {
                return;
            }

            QuestRequirement[] requirements = Definition.Requirements;

            for (int i = 0; i < requirements.Length; i++)
            {
                EventID id = requirements[i].Event;

                if (!id.IsValid)
                {
                    continue;
                }

                EventDatabase.Unsubscribe(id, callback);
            }

            IsCompleted = true;
            isStarted = false;
            callback = null;
        }
        private void OnEvent(EventContext context)
        {
            if (IsCompleted)
            {
                return;
            }

            QuestRequirement[] requirements = Definition.Requirements;

            for (int i = 0; i < requirements.Length; i++)
            {
                QuestRequirement requirement = requirements[i];

                if (requirement.Event != context.ID)
                {
                    continue;
                }

                if (!requirement.IsMatch(context))
                {
                    continue;
                }

                int previous = Progress[i];

                Progress[i] = Math.Min(previous + context.Amount, requirement.Amount);

                if (previous != Progress[i])
                {
                    QuestDatabase.NotifyProgress(this);
                }
            }
        }
    }
}