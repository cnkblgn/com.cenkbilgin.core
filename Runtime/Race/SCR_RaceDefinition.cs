namespace Core.Race
{
    public sealed class RaceDefinition
    {
        public readonly RaceID ID;
        public readonly ulong Tags;
        public readonly RaceVisual Visual;
        internal readonly RaceAction[] Actions;
        internal readonly RaceCompatibility[] Compatibilities;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal RaceDefinition(RaceID id, ulong tags, RaceVisual visual, RaceAction[] actions, RaceCompatibility[] compatibilities)
        {
            ID = id;
            Tags = tags;
            Visual = visual != null ? visual : RaceVisual.NONE;
            Actions = actions ?? RaceAction.NONE;
            Compatibilities = compatibilities ?? RaceCompatibility.NONE;

            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";
        }
        internal RaceDefinition(RaceEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Visual, entry.Actions, entry.Compatibilities) { }
    }
}
