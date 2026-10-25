using System;
using UnityEngine;

namespace Core.Race
{
    [Serializable]
    public struct RaceEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public RaceID ID;
        public RaceTag[] Tags;
        [Expandable, Required] public RaceVisual Visual;
        [Expandable, Required] public Compatibility[] Compatibilities;
        [Expandable, Required] public RaceAction[] Actions;

        public RaceEntry(RaceID id, RaceTag[] tags, RaceVisual visual, Compatibility[] compatibilities, RaceAction[] actions)
        {
            ID = id;
            Tags = tags;
            Visual = visual;
            Compatibilities = compatibilities;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";
        }
#endif
    }
}
