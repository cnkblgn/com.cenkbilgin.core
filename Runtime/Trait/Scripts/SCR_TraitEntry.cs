using System;
using UnityEngine;

namespace Core.Trait
{
    [Serializable]
    public struct TraitEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public TraitID ID;
        [Min(0)] public int Cost;
        [Required, Expandable] public TraitCompatibility[] Compatibilities;
        [Required, Expandable] public TraitAction[] Actions;

        public TraitEntry(TraitID id, int cost, TraitCompatibility[] compatibilities, TraitAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities;
            Cost = cost;
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
