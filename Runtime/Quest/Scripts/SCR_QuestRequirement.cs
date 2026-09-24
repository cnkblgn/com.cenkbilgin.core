using System;
using Core.Actors;
using Core.Event;

namespace Core.Quest
{
    [Serializable]
    public struct QuestRequirement
    {
        public EventID Event;
        public ActorID Actor;
        public ActorTag[] Tags;
        public byte Amount;

        public QuestRequirement(EventID @event, ActorID actor, ActorTag[] tags, byte amount)
        {
            Event = @event;
            Tags = tags;
            Actor = actor;
            Amount = amount;
        }
        public QuestRequirement(EventID @event, ActorID actor, ActorTag[] tags) : this(@event, actor, tags, 1) { }
        public QuestRequirement(EventID @event, ActorID actor, byte amount) : this(@event, actor, null, amount) { }
        public QuestRequirement(EventID @event, ActorID actor) : this(@event, actor, null, 1) { }

        public readonly bool IsMatch(EventContext context)
        {
            if (Actor.IsValid && Actor.Index != context.Actor)
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
