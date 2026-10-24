using System;
using UnityEngine;

namespace Core.Faction
{
    [Serializable]
    public struct FactionEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public FactionID ID;
        public FactionRelation[] Relations;
        [Expandable, Required] public FactionAction[] Actions;
        [Expandable, Required] public FactionCompatibility[] Compatibilities;

        public FactionEntry(FactionID id, FactionRelation[] relations, FactionAction[] actions, FactionCompatibility[] compatibilities)
        {
            ID = id;
            Relations = relations;
            Actions = actions;
            Compatibilities = compatibilities;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Faction{ID.Key}";
            NameID = $"faction.{ID.Key}Name";
            DescID = $"faction.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Faction{ID.Key}";
            NameID = $"faction.{ID.Key}Name";
            DescID = $"faction.{ID.Key}Desc";

            Relations = FactionRelation.GetNormalized(Relations);

            for (int i = 0; i < Relations.Length; i++)
            {
                Relations[i].OnValidate();
            }
        }
#endif
    }
}