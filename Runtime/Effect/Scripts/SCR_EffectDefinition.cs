using System;

namespace Core.Effect
{
    public sealed class EffectDefinition
    {
        public readonly EffectID ID;
        public readonly EffectTag Tag;
        internal readonly int Interval;
        internal readonly EffectCompatibility[] Compatibilities;
        internal readonly EffectAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal EffectDefinition(EffectID id, EffectTag tag, int interval, EffectCompatibility[] compatibilities, EffectAction[] actions)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Effect id is null or empty! please assign new id!") : id;
            Tag = tag;
            IconID = $"Effect{ID.Key}";
            NameID = $"effect.{ID.Key}Name";
            DescID = $"effect.{ID.Key}Desc";
            Compatibilities = compatibilities ?? EffectCompatibility.NONE;
            Actions = actions ?? EffectAction.NONE;
            Interval = interval;
        }
        internal EffectDefinition(EffectEntry entry) : this(entry.ID, entry.Tag, entry.Interval, entry.Compatibilities, entry.Actions) { }
    }
}
