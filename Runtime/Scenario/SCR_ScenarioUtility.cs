using System;
using System.Collections.Generic;

namespace Core.Scenario
{
    public static class ScenarioUtility
    {
        private const string SCENARIO_ID = "scenario_id";

        public static void ExportTo(this ScenarioContainer obj, Dictionary<string, DataNode> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            ScenarioID id = obj.GetScenario();
            data.SetString(id.Key, id.Key);
        }
        public static void ImportFrom<TContext>(this ScenarioContainer obj, Dictionary<string, DataNode> data, TContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            string key = data.GetString(SCENARIO_ID);
            int index = ScenarioDatabase.GetIDIndex(key);
            obj.TrySetScenario(new(key, index), context);
        }
    }
}
