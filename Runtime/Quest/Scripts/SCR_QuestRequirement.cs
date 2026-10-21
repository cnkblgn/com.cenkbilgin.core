using System;
using Core.Actors;
using Core.Event;

namespace Core.Quest
{
    [Serializable]
    public struct QuestRequirement
    {
        public static readonly QuestRequirement[] NONE = new QuestRequirement[0];

        public EventID Event;
        public ActorID Source;
        public ActorTag[] Tags;
        public byte Amount;

        public QuestRequirement(EventID @event, ActorID source, ActorTag[] tags, byte amount)
        {
            Event = @event;
            Tags = tags;
            Source = source;
            Amount = amount;
        }
        public QuestRequirement(EventID @event, ActorID source, ActorTag[] tags) : this(@event, source, tags, 1) { }
        public QuestRequirement(EventID @event, ActorID source, byte amount) : this(@event, source, null, amount) { }
        public QuestRequirement(EventID @event, ActorID source) : this(@event, source, null, 1) { }

        public readonly bool IsMatch(EventContext context)
        {
            if (Event != context.ID)
            {
                return false;
            }

            if (Source.IsValid && Source.Index != context.Source)
            {
                return false;
            }

            ulong requiredTags = Tags.CreateMask();

            if (!context.Tags.HasAll(requiredTags))
            {
                return false;
            }

            return true;
        }
    }
}
