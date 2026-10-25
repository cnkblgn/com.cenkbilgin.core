using System;
using UnityEngine;

namespace Core.Scenario
{
    [Serializable]
    public struct ScenarioEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public ScenarioID ID;
        [Expandable, Required] public Compatibility[] Compatibilities;
        [Expandable, Required] public ScenarioAction[] Actions;

        public ScenarioEntry(ScenarioID id, Compatibility[] compatibilities, ScenarioAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Scenario{ID.Key}";
            NameID = $"scenario.{ID.Key}Name";
            DescID = $"scenario.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Scenario{ID.Key}";
            NameID = $"scenario.{ID.Key}Name";
            DescID = $"scenario.{ID.Key}Desc";
        }
#endif
    }
}
