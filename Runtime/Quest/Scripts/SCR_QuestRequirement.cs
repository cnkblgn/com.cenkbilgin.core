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
        public byte Amount;

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
            Name = $"Event: {Event.Key} | Source: {Source.Key} | Tags: {Tags.CreateMask()} | Amount: {Amount}";
        }
#endif

        public readonly bool IsMatch(EventContext ctx)
        {
            string req = $"Event: {Event} | Source: {Source} | Tags: {Tags.CreateMask()} | Amount: {Amount}";
            string @event = $"Event: {ctx.ID.Key} | Source: {ctx.Source} | Tags: {ctx.Tags} | Amount: {ctx.Amount}";

            UnityEngine.Debug.Log($"IsMatch? Req: {req} | Evnt: {@event}");

            if (Event != ctx.ID)
            {
                return false;
            }

            if (Source.IsValid && Source.Index != ctx.Source)
            {
                return false;
            }

            ActorMask mask = Tags.CreateMask();

            Debug.Log("IsMatch? Mask fail test");

            if (!ctx.Tags.HasAll(mask.Value))
            {
                return false;
            }

            Debug.Log("IsMatched!! successfull");

            return true;
        }
    }
}
