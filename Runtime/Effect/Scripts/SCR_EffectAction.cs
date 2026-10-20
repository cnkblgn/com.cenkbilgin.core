using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectAction : ScriptableObject
    {
        internal static readonly EffectAction[] NONE = new EffectAction[0];

        public string Description { get; }
        public abstract void Tick(GameObject actor, ref EffectInstance instance);
        public abstract void Apply(GameObject actor, ref EffectInstance instance);
        public abstract void Removed(GameObject actor, ref EffectInstance instance);
    }
}
