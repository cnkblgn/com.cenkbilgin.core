using System;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    public sealed class EffectDefinition
    {
        public readonly EffectID ID;
        public readonly EffectTag Tag;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly EffectAction Action;
        public readonly int Interval;

        internal EffectDefinition(EffectID id, EffectTag tag, IconID iconID, LocalizedID nameID, EffectAction action, int interval)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Effect id is null or empty! please assign new id!") : id;
            Tag = tag;
            IconID = iconID;
            NameID = nameID;
            Action = action ?? throw new ArgumentNullException($"Effect action cannot be null! please assign action via effect entry! {nameof(action)}");
            Interval = interval;
        }
        internal EffectDefinition(EffectEntry entry) : this(entry.ID, entry.Tag, entry.IconID, entry.NameID, entry.Action, entry.Interval) { }
    }
}
