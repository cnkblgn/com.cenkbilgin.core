using System;
using Core.Actors;

namespace Core.Effect
{
    using static CoreUtility;

    [Serializable]
    public abstract class EffectAction
    {
        public abstract string Description { get; }
        public abstract void Tick(Actor actor, ref EffectInstance instance);
        public abstract void Apply(Actor actor, ref EffectInstance instance);
        public abstract void Removed(Actor actor, ref EffectInstance instance);

#if UNITY_EDITOR
        public virtual void OnValidate() { }
#endif
    }

    [Serializable]
    public sealed class EffectActionNone : EffectAction
    {
        public override string Description => STRING_EMPTY;
        public override void Tick(Actor actor, ref EffectInstance instance) { }
        public override void Apply(Actor actor, ref EffectInstance instance) { }
        public override void Removed(Actor actor, ref EffectInstance instance) { }
    }
}
