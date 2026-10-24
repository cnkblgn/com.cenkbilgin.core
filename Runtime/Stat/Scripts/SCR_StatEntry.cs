using System;
using UnityEngine;

namespace Core.Stat
{
    [Serializable]
    public struct StatEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
        [SerializeField, ReadOnly] private string IconID;
        [SerializeField, ReadOnly] private string NameID;
        [SerializeField, ReadOnly] private string DescID;
#endif

        public StatID ID;
        public StatTag Tag;
        public float Default;
        public float Min;
        public float Max;

        public StatEntry(StatID id, StatTag tag, float @default, float min, float max)
        {
            ID = id;
            Tag = tag;
            Default = @default;
            Min = min;
            Max = max;

#if UNITY_EDITOR
            Name = ID.Key;
            IconID = $"Stat{ID.Key}";
            NameID = $"stat.{ID.Key}Name";
            DescID = $"stat.{ID.Key}Desc";
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            IconID = $"Stat{ID.Key}";
            NameID = $"stat.{ID.Key}Name";
            DescID = $"stat.{ID.Key}Desc";
        }
#endif
    }
}