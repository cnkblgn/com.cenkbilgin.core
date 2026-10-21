using System;

namespace Core.Effect
{
    public sealed class EffectDefinition
    {
        public readonly EffectID ID;
        public readonly EffectTag Tag;
        internal readonly int Interval;
        internal readonly EffectValidator[] Validators;
        internal readonly EffectAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal EffectDefinition(EffectID id, EffectTag tag, int interval, EffectValidator[] validators, EffectAction[] actions)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Effect id is null or empty! please assign new id!") : id;
            Tag = tag;
            IconID = $"Icon{ID.Key}";
            NameID = $"effect.{ID.Key}.name";
            DescID = $"effect.{ID.Key}.desc";
            Validators = validators ?? EffectValidator.NONE;
            Actions = actions ?? EffectAction.NONE;
            Interval = interval;
        }
        internal EffectDefinition(EffectEntry entry) : this(entry.ID, entry.Tag, entry.Interval, entry.Validators, entry.Actions) { }
    }
}
