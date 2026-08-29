using System;
using UnityEngine;
using Core.Graphics;
using Core.Localization;

namespace Core.Effect
{
    [Serializable]
    public struct EffectEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

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

#if UNITY_EDITOR
            Name = NameID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = NameID.Key;

            for (int i = 0; i < Actions.Length; i++)
            {
                Actions[i].OnValidate();
            }
        }
#endif
    }
}