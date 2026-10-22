using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectCompatibility : ScriptableObject
    {
        public static readonly EffectCompatibility[] NONE = new EffectCompatibility[0];

        public abstract bool IsCompatibleWith(EffectID idA, EffectID idB);
        public abstract bool IsCompatibleWith(EffectID id, IReadOnlyList<EffectInstance> effects, object actor);
    }
}
