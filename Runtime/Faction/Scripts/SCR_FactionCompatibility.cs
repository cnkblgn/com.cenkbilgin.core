using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionCompatibility : ScriptableObject
    {
        public static readonly FactionCompatibility[] NONE = new FactionCompatibility[0];
    }
    public abstract class FactionCompatibility<TContext> : ScriptableObject
    {
        public abstract bool IsCompatibleWith(TContext ctx);
    }
}
