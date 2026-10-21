using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitAction : ScriptableObject
    {
        public static readonly TraitAction[] NONE = new TraitAction[0];

        public string Description { get; }

        public abstract void Apply(ref TraitInstance instance, object actor);
        public abstract void Remove(ref TraitInstance instance, object actor);
    }
}
