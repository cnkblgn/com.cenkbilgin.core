using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionCompatibility : ScriptableObject
    {
        public static readonly FactionCompatibility[] NONE = new FactionCompatibility[0];

        public abstract bool IsCompatibleWith(FactionID id, object actor);
    }
}
