using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectAction : ScriptableObject
    {
        public static readonly EffectAction[] NONE = new EffectAction[0];
    }
    public abstract class EffectAction<TContext> : EffectAction
    {
        public abstract string Description { get; }

        public abstract void Tick(ref EffectInstance instance, TContext context);
        public abstract void Apply(ref EffectInstance instance, TContext context);
        public abstract void Remove(ref EffectInstance instance, TContext context);
    }
}
