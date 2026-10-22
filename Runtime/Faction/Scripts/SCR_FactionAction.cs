using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionAction : ScriptableObject
    {
        public static readonly FactionAction[] NONE = new FactionAction[0];

        public abstract void Apply(FactionID id, object actor);
        public abstract void Remove(FactionID id, object actor);
    }
}
