using System;

namespace Core.Stat
{
    [Serializable]
    public struct StatEntry
    {
        [Info("Please generate id if its not visible")] public StatID ID;
        public float Default;
        public float Min;
        public float Max;

        public StatEntry(StatID id, float @default, float min, float max)
        {
            ID = id;
            Default = @default;
            Min = min;
            Max = max;
        }
    }
}