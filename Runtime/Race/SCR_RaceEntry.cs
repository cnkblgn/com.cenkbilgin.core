using System;
using UnityEngine;

namespace Core.Race
{
    [Serializable]
    public struct RaceEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public RaceID ID;
        public RaceTag[] Tags;
        [Required, Expandable] public RaceVisual Visual;
        [Required, Expandable] public RaceCompatibility[] Compatibilities;
        [Required, Expandable] public RaceAction[] Actions;

        public RaceEntry(RaceID id, RaceTag[] tags, RaceVisual visual, RaceCompatibility[] compatibilities, RaceAction[] actions)
        {
            ID = id;
            Tags = tags;
            Visual = visual;
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
