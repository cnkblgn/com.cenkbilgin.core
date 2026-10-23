using UnityEngine;

namespace Core.Scenario
{
    public abstract class ScenarioCompatibility : ScriptableObject
    {
        public static readonly ScenarioCompatibility[] NONE = new ScenarioCompatibility[] { };

        public virtual bool IsCompatibleWith(ScenarioID id, object context) => true;
        public virtual bool IsCompatibleWith(ScenarioID id, int context) => true;
        public virtual bool IsCompatibleWith(ScenarioID id, ulong context) => true;
    }
}
