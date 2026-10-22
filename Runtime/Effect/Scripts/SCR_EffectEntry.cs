using System;
using UnityEngine;

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
        [Min(0)] public int Interval;
        [Required, Expandable] public EffectCompatibility[] Compatibilities;
        [Required, Expandable] public EffectAction[] Actions;

        public EffectEntry(EffectID id, EffectTag tag, int interval, EffectCompatibility[] compatibilities, EffectAction[] actions)
        {
            ID = id;
            Tag = tag;
            Interval = interval;
            Compatibilities = compatibilities;
            Actions = actions;

            Name = ID.Key;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
        }
#endif
    }
}