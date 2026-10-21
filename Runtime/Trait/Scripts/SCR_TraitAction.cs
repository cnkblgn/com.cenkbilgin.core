using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitAction : ScriptableObject
    {
        public static readonly TraitAction[] NONE = new TraitAction[0];

        public string Description { get; }

        public abstract void Apply(GameObject actor, ref TraitInstance instance);
        public abstract void Remove(GameObject actor, ref TraitInstance instance);
    }
}
