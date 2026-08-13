using System;
using Core.Localization;

namespace Core.Stat
{
    public sealed class StatDefinition
    {
        public readonly StatID ID;
        public readonly StatTag Tag;
        public readonly LocalizedID NameID;
        public readonly float Default;
        public readonly float Min;
        public readonly float Max;

        public StatDefinition(StatID id, StatTag tag, LocalizedID nameID, float @default, float min, float max)
        {
            ID = !id.IsValid ? throw new NullReferenceException("Stat id is null or empty! please assign new id!") : id;
            NameID = nameID;
            Default = @default;
            Min = min;
            Max = max;
        }
        public StatDefinition(StatEntry entry) : this(entry.ID, entry.Tag, entry.NameID, entry.Default, entry.Min, entry.Max) { }
    }
}