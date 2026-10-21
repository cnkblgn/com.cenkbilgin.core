using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitValidator : ScriptableObject
    {
        public static readonly TraitValidator[] NONE = new TraitValidator[] { };

        public abstract bool IsCompatibleWith(TraitID idA, TraitID idB);
        public abstract bool IsCompatibleWith(TraitID id, IReadOnlyList<TraitInstance> traits, object actor);
    }
}
