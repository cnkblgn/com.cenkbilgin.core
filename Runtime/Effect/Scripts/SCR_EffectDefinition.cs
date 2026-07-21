using System;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    public sealed class EffectDefinition
    {
        public readonly EffectID ID;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly EffectAction Action;
        public readonly EffectTag Tag;
        public readonly int Interval;

        public EffectDefinition(EffectID id, IconID iconID, LocalizedID nameID, EffectAction action, EffectTag tag, int interval)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Effect id is null or empty! please assign new id!") : id;
            IconID = iconID;
            NameID = nameID;
            Tag = tag;
            Action = action ?? throw new ArgumentNullException($"Effect action cannot be null! please assign action via effect entry! {nameof(action)}");
            Interval = interval;
        }
    }
}
