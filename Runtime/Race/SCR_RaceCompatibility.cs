using UnityEngine;

namespace Core.Race
{
    public abstract class RaceCompatibility : ScriptableObject
    {
        public static readonly RaceCompatibility[] NONE = new RaceCompatibility[] { };
    }
    public abstract class RaceCompatibility<TContext> : RaceCompatibility
    {
        public abstract bool IsCompatibleWith(RaceID id, TContext context);
    }
}
