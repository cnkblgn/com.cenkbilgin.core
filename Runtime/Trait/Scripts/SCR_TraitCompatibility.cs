using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitCompatibility : ScriptableObject 
    {
        public static readonly TraitCompatibility[] NONE = new TraitCompatibility[] { };
    }
    public abstract class TraitCompatibility<TContext> : TraitCompatibility
    {
        public abstract bool IsCompatibleWith(TraitID id, TContext context);
    }
}
