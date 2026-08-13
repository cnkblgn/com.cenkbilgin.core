using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    [Serializable]
    public struct EffectEntry
    {
        [HideInInspector] public string Name;

        [Info("Please generate id if its not visible")] 
        public EffectID ID;
        public EffectTag Tag;
        public LocalizedID NameID;
        public IconID IconID;
        [Min(0)] public int Interval;
        [SerializeReference, Reference] public EffectAction Action;

        public EffectEntry(EffectID id, EffectTag tag, LocalizedID nameID, IconID iconID, int interval, EffectAction action)
        {
            ID = id;
            Tag = tag;
            NameID = nameID;
            IconID = iconID;
            Interval = interval;
            Action = action;

            Name = ID.Key;
        }
    }
}