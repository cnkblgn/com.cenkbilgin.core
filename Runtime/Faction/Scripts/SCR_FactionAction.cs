using UnityEngine;

namespace Core.Faction
{
    public abstract class FactionAction : ScriptableObject
    {
        public static readonly FactionAction[] NONE = new FactionAction[0];
    }
    public abstract class FactionAction<TContext> : FactionAction
    {
        public abstract string Description { get; }

        public abstract void Apply(FactionID id, TContext context);
        public abstract void Remove(FactionID id, TContext context);
    }
}
