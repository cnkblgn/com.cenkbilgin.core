namespace Core.Race
{
    public sealed class RaceDefinition
    {
        public readonly RaceID ID;
        public readonly RaceMask Mask;
        public readonly RaceVisual Visual;
        internal readonly RaceAction[] Actions;
        internal readonly RaceCompatibility[] Compatibilities;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal RaceDefinition(RaceID id, RaceMask mask, RaceVisual visual, RaceAction[] actions, RaceCompatibility[] compatibilities)
        {
            ID = id;
            Mask = mask;
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
