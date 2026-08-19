using System;
using UnityEngine;
using Core.Localization;

namespace Core.Stat
{
    [Serializable]
    public struct StatEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        public StatID ID;
        public StatTag Tag;
        public LocalizedID NameID;
        public float Default;
        public float Min;
        public float Max;

        public StatEntry(StatID id, StatTag tag, LocalizedID nameID, float @default, float min, float max)
        {
            ID = id;
            Tag = tag;
            NameID = nameID;
            Default = @default;
            Min = min;
            Max = max;

#if UNITY_EDITOR
            Name = NameID.Key;
#endif
        }
    }
}