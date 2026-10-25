namespace Core.Race
{
    public sealed class RaceDefinition
    {
        public readonly RaceID ID;
        public readonly RaceMask Mask;
        public readonly RaceVisual Visual;
        internal readonly Compatibility[] Compatibilities;
        internal readonly RaceAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal RaceDefinition(RaceID id, RaceMask mask, RaceVisual visual, Compatibility[] compatibilities, RaceAction[] actions)
        {
            ID = id;
            Mask = mask;
            Visual = visual != null ? visual : RaceVisual.NONE;
            Actions = actions ?? RaceAction.NONE;
            Compatibilities = compatibilities ?? Compatibility.NONE;

            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";
        }
        internal RaceDefinition(RaceEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Visual, entry.Compatibilities, entry.Actions) { }
    }
}
