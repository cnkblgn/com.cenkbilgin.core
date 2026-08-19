using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Trait
{
    [Serializable]
    public struct TraitEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public TraitID ID;
        public TraitID[] IncompatibleIDs;
        public IconID IconID;
        public LocalizedID NameID;
        public LocalizedID DescID;

        [Min(0)] public int Cost;
        [SerializeReference, Reference] public TraitAction[] Actions;

        public TraitEntry(TraitID id, TraitID[] incompatibleIDs, IconID iconID, LocalizedID nameID, LocalizedID descID, int cost, TraitAction[] actions)
        {
            ID = id;
            IncompatibleIDs = incompatibleIDs;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Cost = cost;
            Actions = actions;

#if UNITY_EDITOR
            Name = id.Key;
#endif
        }
    }
}
