using System;
using UnityEngine;

namespace Core.Trait
{
    [Serializable]
    public struct TraitEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public TraitID ID;
        [Min(0)] public int Cost;
        [Expandable, Required] public Compatibility[] Compatibilities;
        [Expandable, Required] public TraitAction[] Actions;

        public TraitEntry(TraitID id, int cost, Compatibility[] compatibilities, TraitAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities;
            Cost = cost;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Trait{ID.Key}";
            NameID = $"trait.{ID.Key}Name";
            DescID = $"trait.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Trait{ID.Key}";
            NameID = $"trait.{ID.Key}Name";
            DescID = $"trait.{ID.Key}Desc";
        }
#endif
    }
}
