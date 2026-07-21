using System;
using UnityEngine;
using Core.Actors;

namespace Core.Effect
{
    [Serializable]
    public abstract class EffectAction
    {
        public abstract void Tick(Actor entity, ref EffectInstance instance);
        public abstract void Apply(Actor entity, ref EffectInstance instance);
        public abstract void Removed(Actor entity, ref EffectInstance instance);
    }
}
