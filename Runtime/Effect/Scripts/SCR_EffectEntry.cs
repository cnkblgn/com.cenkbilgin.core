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

        public EffectID ID;
        public EffectTag Tag;
        public LocalizedID NameID;
        public IconID IconID;
        [Min(0)] public int Interval;
        [SerializeReference, Reference] public EffectAction[] Actions;

        public EffectEntry(EffectID id, EffectTag tag, LocalizedID nameID, IconID iconID, int interval, EffectAction[] actions)
        {
            ID = id;
            Tag = tag;
            NameID = nameID;
            IconID = iconID;
            Interval = interval;
            Actions = actions;
            Name = NameID.Key;
        }
    }
}