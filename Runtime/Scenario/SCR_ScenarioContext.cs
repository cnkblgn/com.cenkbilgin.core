using UnityEngine;

namespace Core.Scenario
{
    public readonly struct ScenarioContext
    {
        public readonly ScenarioID ID;

        public ScenarioContext(ScenarioID id)
        {
            ID = id;
        }
    }
}