using UnityEngine;

namespace Core.Race
{
    public abstract class RaceCompatibility : ScriptableObject
    {
        public static readonly RaceCompatibility[] NONE = new RaceCompatibility[] { };

        public virtual bool IsCompatibleWith(RaceID id, object context) => true;
        public virtual bool IsCompatibleWith(RaceID id, int context) => true;
        public virtual bool IsCompatibleWith(RaceID id, ulong context) => true;
    }
}
