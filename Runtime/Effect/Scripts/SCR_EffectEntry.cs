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
        public LocalizedID NameID;
        public IconID IconID;
        public EffectTag Tag;
        [Min(0)] public int Interval;
        [SerializeReference, Reference] public EffectAction Action;

        public EffectEntry(EffectID id, LocalizedID nameID, IconID iconID, EffectTag tag, int interval, EffectAction action)
        {
            ID = id;
            NameID = nameID;
            IconID = iconID;
            Tag = tag;
            Interval = interval;
            Action = action;

            Name = ID.Key;
        }
    }
}