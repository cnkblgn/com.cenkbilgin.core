using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectCompatibility : ScriptableObject
    {
        public static readonly EffectCompatibility[] NONE = new EffectCompatibility[0];

        public virtual bool IsCompatibleWith(EffectID idA, EffectID idB) => true;
        public virtual bool IsCompatibleWith(EffectID id, object context) => true;
        public virtual bool IsCompatibleWith(EffectID id, int context) => true;
        public virtual bool IsCompatibleWith(EffectID id, ulong context) => true;
    }
}
