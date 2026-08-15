using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Trait
{
    [Serializable]
    public struct TraitEntry
    {
        [HideInInspector] public string Name;

        [Info("Please generate id if its not visible")] 
        public TraitID ID;
        public TraitID[] IncompatibleIDs;
        public IconID IconID;
        public LocalizedID NameID;
        public LocalizedID DescID;

        [Min(0)] public int Cost;
        [SerializeReference, Reference] public TraitAction[] Actions;

        public TraitEntry(string name, TraitID id, TraitID[] incompatibleIDs, IconID iconID, LocalizedID nameID, LocalizedID descID, int cost, TraitAction[] actions)
        {
            Name = name;
            ID = id;
            IncompatibleIDs = incompatibleIDs;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
            Cost = cost;
            Actions = actions;
        }
    }
}
