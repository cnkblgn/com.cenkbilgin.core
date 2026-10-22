using System;
using UnityEngine;

namespace Core.Scenario
{
    [Serializable]
    public struct ScenarioEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public ScenarioID ID;
        [Required, Expandable] public ScenarioCompatibility[] Compatibilities;
        [Required, Expandable] public ScenarioAction[] Actions;

        public ScenarioEntry(ScenarioID id, ScenarioCompatibility[] compatibilities, ScenarioAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
        }
#endif
    }
}
