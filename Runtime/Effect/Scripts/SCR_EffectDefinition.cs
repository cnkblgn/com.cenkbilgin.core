using System;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    public sealed class EffectDefinition
    {
        public readonly EffectID ID;
        public readonly EffectTag Tag;
        public readonly LocalizedID NameID;
        public readonly IconID IconID;
        internal readonly EffectAction[] Actions;
        internal readonly int Interval;

        internal EffectDefinition(EffectID id, EffectTag tag, LocalizedID nameID, IconID iconID, EffectAction[] actions, int interval)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Effect id is null or empty! please assign new id!") : id;
            Tag = tag;
            IconID = iconID;
            NameID = nameID;
            Actions = actions ?? (new EffectAction[] { new EffectActionNone() });
            Interval = interval;
        }
        internal EffectDefinition(EffectEntry entry) : this(entry.ID, entry.Tag, entry.NameID, entry.IconID, entry.Actions, entry.Interval) { }
    }
}
