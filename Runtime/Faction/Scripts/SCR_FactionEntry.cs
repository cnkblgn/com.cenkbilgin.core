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
        [Required] public FactionValidator[] Validators;

        public FactionEntry(FactionID id, FactionRelation[] relations, FactionValidator[] validators)
        {
            ID = id;
            Relations = relations;
            Validators = validators;

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