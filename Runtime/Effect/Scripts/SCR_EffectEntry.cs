using System;
using UnityEngine;

namespace Core.Effect
{
    [Serializable]
    public struct EffectEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public EffectID ID;
        public EffectTag Tag;
        [Min(0)] public int Interval;
        [Expandable, Required] public EffectCompatibility[] Compatibilities;
        [Expandable, Required] public EffectAction[] Actions;

        public EffectEntry(EffectID id, EffectTag tag, int interval, EffectCompatibility[] compatibilities, EffectAction[] actions)
        {
            ID = id;
            Tag = tag;
            Interval = interval;
            Compatibilities = compatibilities;
            Actions = actions;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Effect{ID.Key}";
            NameID = $"effect.{ID.Key}Name";
            DescID = $"effect.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Effect{ID.Key}";
            NameID = $"effect.{ID.Key}Name";
            DescID = $"effect.{ID.Key}Desc";
        }
#endif
    }
}