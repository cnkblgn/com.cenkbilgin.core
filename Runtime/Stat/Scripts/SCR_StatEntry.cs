using System;

namespace Core.Stat
{
    [Serializable]
    public struct StatEntry
    {
        [Required] public string Key;
        public float Default;
        public float Min;
        public float Max;

        public StatEntry(string key, float @default, float min, float max)
        {
            Key = key;
            Default = @default;
            Min = min;
            Max = max;
        }
        public StatEntry(StatEntry entry) : this(entry.Key, entry.Default, entry.Min, entry.Max) { }
    }
}