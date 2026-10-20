using System.Collections.Generic;
using UnityEngine;

namespace Core.Trait
{
    public abstract class TraitValidator : ScriptableObject
    {
        internal static readonly TraitValidator[] NONE = new TraitValidator[] { };

        public abstract bool IsInCompatibleWith(TraitID idA, TraitID idB);
        public abstract bool IsInCompatibleWith(TraitID id, IReadOnlyList<TraitInstance> traits, GameObject actor);
    }
}
