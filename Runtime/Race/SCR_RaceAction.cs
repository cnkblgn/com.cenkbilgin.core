using UnityEngine;

namespace Core.Race
{
    public abstract class RaceAction : ScriptableObject
    {
        public static readonly RaceAction[] NONE = new RaceAction[0];
    }

    public abstract class RaceAction<TContext> : RaceAction
    {
        public abstract string Description { get; }

        public abstract void Apply(RaceID id, TContext context);
        public abstract void Remove(RaceID id, TContext context);
    }
}
