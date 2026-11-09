using System;
using UnityEngine;
using Core.Actors;
using Core.Event;

namespace Core.Quest
{
    [Serializable]
    public struct QuestRequirement
    {
        public static readonly QuestRequirement[] NONE = new QuestRequirement[0];

        [HideInInspector] public string Name;
        public EventID Event;
        public ActorID Source;
        public ActorTag[] Tags;
        [Min(1)] public byte Amount;

        public QuestRequirement(EventID @event, ActorID source, ActorTag[] tags, byte amount)
        {
            Name = "";
            Event = @event;
            Tags = tags;
            Source = source;
            Amount = amount;
        }
        public QuestRequirement(EventID @event, ActorID source, ActorTag[] tags) : this(@event, source, tags, 1) { }
        public QuestRequirement(EventID @event, ActorID source, byte amount) : this(@event, source, null, amount) { }
        public QuestRequirement(EventID @event, ActorID source) : this(@event, source, null, 1) { }

#if UNITY_EDITOR
        internal void Validate()
        {
            Name = $"Event: {Event.Key} | Source: {Source.Key} | Tags: {Tags.CreateMask().Value} | Amount: {Amount}";
        }
#endif

        public readonly bool IsMatch(EventContext ctx)
        {
            if (Event != ctx.ID)
            {
                return false;
            }

            if (Source.IsValid && Source.Index != ctx.Source)
            {
                return false;
            }

            ActorMask mask = Tags.CreateMask();

            if (!ctx.Tags.HasAll(mask.Value))
            {
                return false;
            }

            return true;
        }
    }
}
