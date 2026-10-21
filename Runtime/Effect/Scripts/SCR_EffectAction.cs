using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectAction : ScriptableObject
    {
        public static readonly EffectAction[] NONE = new EffectAction[0];

        public string Description { get; }
        public abstract void Tick(ref EffectInstance instance, object actor);
        public abstract void Apply(ref EffectInstance instance, object actor);
        public abstract void Removed(ref EffectInstance instance, object actor);
    }
}
