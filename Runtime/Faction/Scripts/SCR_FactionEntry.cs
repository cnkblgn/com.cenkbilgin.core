using System;
using UnityEngine;

namespace Core.Faction
{
    [Serializable]
    public struct FactionEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif
        public FactionID ID;
        public FactionRelation[] Relations;
        [Required, Expandable] public FactionAction[] Actions;
        [Required, Expandable] public FactionCompatibility[] Compatibilities;

        public FactionEntry(FactionID id, FactionRelation[] relations, FactionAction[] actions, FactionCompatibility[] compatibilities)
        {
            ID = id;
            Relations = relations;
            Actions = actions;
            Compatibilities = compatibilities;

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;

            Relations = FactionRelation.GetNormalized(Relations);

            for (int i = 0; i < Relations.Length; i++)
            {
                Relations[i].OnValidate();
            }
        }
#endif
    }
}