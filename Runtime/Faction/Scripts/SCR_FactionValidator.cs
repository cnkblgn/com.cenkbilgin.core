using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionValidator : ScriptableObject
    {
        public static readonly FactionValidator[] NONE = new FactionValidator[0];

        public abstract bool IsCompatibleWith(FactionID id, object actor);
    }
}
