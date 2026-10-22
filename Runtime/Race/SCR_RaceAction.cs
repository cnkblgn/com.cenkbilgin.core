using UnityEngine;

namespace Core.Race
{
    public abstract class RaceAction : ScriptableObject
    {
        public static readonly RaceAction[] NONE = new RaceAction[0];

        public abstract void Apply(RaceID id, object actor);
        public abstract void Remove(RaceID id, object actor);
    }
}
