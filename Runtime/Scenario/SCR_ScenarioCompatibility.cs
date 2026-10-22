using UnityEngine;

namespace Core.Scenario
{
    public abstract class ScenarioCompatibility : ScriptableObject
    {
        public static readonly ScenarioCompatibility[] NONE = new ScenarioCompatibility[] { };

        public abstract bool IsCompatibleWith(ScenarioID id, object actor);
    }
}
