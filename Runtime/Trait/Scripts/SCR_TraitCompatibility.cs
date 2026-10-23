using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitCompatibility : ScriptableObject
    {
        public static readonly TraitCompatibility[] NONE = new TraitCompatibility[] { };

        public virtual bool IsCompatibleWith(TraitID idA, TraitID idB) => true;
        public virtual bool IsCompatibleWith(TraitID id, object context) => true;
        public virtual bool IsCompatibleWith(TraitID id, int context) => true;
        public virtual bool IsCompatibleWith(TraitID id, ulong context) => true;
    }
}
