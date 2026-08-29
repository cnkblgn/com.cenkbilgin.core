using System;
using Core.Actors;

namespace Core.Trait
{
    using static CoreUtility;

    [Serializable]
    public abstract class TraitAction
    {
        public abstract string Description { get; }
        public abstract void Apply(Actor actor, ref TraitInstance instance);
        public abstract void Remove(Actor actor, ref TraitInstance instance);

#if UNITY_EDITOR
        public virtual void OnValidate() { }
#endif
    }

    [Serializable]
    public sealed class TraitActionNone : TraitAction
    {
        public override string Description => STRING_EMPTY;
        public override void Apply(Actor actor, ref TraitInstance instance) { }
        public override void Remove(Actor actor, ref TraitInstance instance) { }
    }
}
