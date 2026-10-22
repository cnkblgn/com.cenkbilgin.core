namespace Core.Stat
{
    public sealed class StatDefinition
    {
        public readonly StatID ID;
        public readonly StatTag Tag;
        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;
        public readonly float Default;
        public readonly float Min;
        public readonly float Max;

        internal StatDefinition(StatID id, StatTag tag, float @default, float min, float max)
        {
            ID = id;
            Tag = tag;
            IconID = $"Stat{ID.Key}";
            NameID = $"stat.{ID.Key}.name";
            DescID = $"stat.{ID.Key}.desc";
            Default = @default;
            Min = min;
            Max = max;
        }
        internal StatDefinition(StatEntry entry) : this(entry.ID, entry.Tag, entry.Default, entry.Min, entry.Max) { }
    }
}