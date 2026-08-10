using System;
using Core.Actors;

namespace Core.Trait
{
    using static CoreUtility;

    [Serializable]
    public abstract class TraitAction
    {
        public abstract string Description { get; }
        public abstract void Apply(Actor character, ref TraitInstance instance);
        public abstract void Remove(Actor character, ref TraitInstance instance);
    }

    [Serializable]
    public sealed class TraitActionEmpty : TraitAction
    {
        public override string Description => STRING_EMPTY;
        public override void Apply(Actor character, ref TraitInstance instance) { }
        public override void Remove(Actor character, ref TraitInstance instance) { }
    }
}
