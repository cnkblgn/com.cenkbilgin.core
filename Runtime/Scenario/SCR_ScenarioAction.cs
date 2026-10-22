using UnityEngine;

namespace Core.Scenario
{
    public abstract class ScenarioAction : ScriptableObject
    {
        public static readonly ScenarioAction[] NONE = new ScenarioAction[0];

        public abstract void Apply(ScenarioID id, object actor);
        public abstract void Remove(ScenarioID id, object actor);
    }
}
