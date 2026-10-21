using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    public abstract class EffectValidator : ScriptableObject
    {
        public static readonly EffectValidator[] NONE = new EffectValidator[0];

        public abstract bool IsCompatibleWith(EffectID idA, EffectID idB);
        public abstract bool IsCompatibleWith(EffectID id, IReadOnlyList<EffectInstance> effects, object actor);
    }
}
