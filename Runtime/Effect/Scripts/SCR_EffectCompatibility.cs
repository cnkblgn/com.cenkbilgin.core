using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectCompatibility : ScriptableObject
    {
        public static readonly EffectCompatibility[] NONE = new EffectCompatibility[0];
    }
    public abstract class EffectCompatibility<TContext> : EffectCompatibility
    {
        public abstract bool IsCompatibleWith(TContext ctx);
    }
}
