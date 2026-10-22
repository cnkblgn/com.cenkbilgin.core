using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitCompatibility : ScriptableObject
    {
        public static readonly TraitCompatibility[] NONE = new TraitCompatibility[] { };

        public abstract bool IsCompatibleWith(TraitID idA, TraitID idB);
        public abstract bool IsCompatibleWith(TraitID id, IReadOnlyList<TraitInstance> traits, object actor);
    }
}
