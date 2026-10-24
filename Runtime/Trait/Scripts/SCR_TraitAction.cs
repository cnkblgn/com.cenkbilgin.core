using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitAction : ScriptableObject
    {
        public static readonly TraitAction[] NONE = new TraitAction[0];
    }
    public abstract class TraitAction<TContext> : ScriptableObject
    {
        public string Description { get; }

        public abstract void Apply(ref TraitInstance instance, TContext context);
        public abstract void Remove(ref TraitInstance instance, TContext context);
    }
}
