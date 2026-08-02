using Core.Actors;
using System;

namespace Core.Quest
{
    [Serializable]
    public struct QuestRequirement
    {
        public QuestEvent Event;
        public ActorID ID;
        public ActorTag[] Tags;
        public byte Amount;

        public QuestRequirement(QuestEvent @event, ActorID id, ActorTag[] tags, byte amount)
        {
            Event = @event;
            Tags = tags;
            ID = id;
            Amount = amount;
        }
        public QuestRequirement(QuestEvent @event, ActorID id, ActorTag[] tags) : this(@event, id, tags, 1) { }
        public QuestRequirement(QuestEvent @event, ActorID id, byte amount) : this(@event, id, null, amount) { }
        public QuestRequirement(QuestEvent @event, ActorID id) : this(@event, id, null, 1) { }

        public readonly bool IsMatch(QuestEvent @event, ActorID id, ulong tags)
        {
            if (Event != @event)
            {
                return false;
            }

            if (tags != 0 && !tags.HasAll(Tags.CreateMask()))
            {
                return false;
            }

            if (ID.IsValid && ID != id)
            {
                return false;
            }

            return true;
        }
    }
}
