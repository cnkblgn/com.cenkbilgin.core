using UnityEngine;

namespace Core.Scenario
{
    public abstract class ScenarioAction : ScriptableObject
    {
        public static readonly ScenarioAction[] NONE = new ScenarioAction[0];
    }

    public abstract class ScenarioAction<TContext> : ScenarioAction
    {
        public abstract string Description { get; }

        public abstract void Apply(ScenarioID id, TContext context);
        public abstract void Remove(ScenarioID id, TContext context);
    }
}
