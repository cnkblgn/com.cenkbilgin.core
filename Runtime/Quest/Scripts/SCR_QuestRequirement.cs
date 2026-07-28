using Core.Actors;
using System;

namespace Core.Quest
{
    [Serializable]
    public struct QuestRequirement
    {
        public QuestEvent Event;
        public ActorID ID;
        public ulong Tags;
        public byte Amount;

        public QuestRequirement(QuestEvent @event, ActorID id, ActorTag[] tags, byte amount)
        {
            Event = @event;
            Tags = tags != null ? tags.CreateMask() : 0;
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

            if (tags != 0 && !tags.HasAll(Tags))
            {
                return false;
            }

            if (ID != ActorID.NONE && ID != id)
            {
                return false;
            }

            return true;
        }
    }
}
