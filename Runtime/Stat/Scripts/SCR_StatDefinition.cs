using System;

namespace Core.Stat
{
    public sealed class StatDefinition
    {
        public readonly StatID ID;
        public readonly float Default;
        public readonly float Min;
        public readonly float Max;

        public StatDefinition(StatID id, float @default, float min, float max)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Stat id is null or empty! please assign new id!") : id;
            Default = @default;
            Min = min;
            Max = max;
        }
    }
}