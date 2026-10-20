using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionValidator : ScriptableObject
    {
        internal static readonly FactionValidator[] NONE = new FactionValidator[0];

        public abstract bool IsInCompatibleWith(FactionID id, GameObject actor);
    }
}
