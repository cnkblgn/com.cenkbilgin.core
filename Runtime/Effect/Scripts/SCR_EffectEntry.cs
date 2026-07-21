using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    [Serializable]
    public struct EffectEntry
    {
        [Header("_")]
        [Required] public string Key;
        public LocalizedID NameID;
        public IconID IconID;

        [Header("_")]
        public EffectTag Tag;
        [Min(0)] public int Interval;

        [Header("_")]
        [SerializeReference, Reference] public EffectAction Action;

        public EffectEntry(string key, LocalizedID nameID, IconID iconID, EffectTag tag, int interval, EffectAction action)
        {
            Key = key;
            NameID = nameID;
            IconID = iconID;
            Tag = tag;
            Interval = interval;
            Action = action;
        }
        public EffectEntry(EffectEntry entry) : this(entry.Key, entry.NameID, entry.IconID, entry.Tag, entry.Interval, entry.Action) { }
    }
}