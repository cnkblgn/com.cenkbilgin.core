namespace Core.Race
{
    public sealed class RaceDefinition
    {
        public readonly RaceID ID;
        internal readonly RaceVisual Visual;
        internal readonly RaceAction[] Actions;
        internal readonly RaceCompatibility[] Compatibilities;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal RaceDefinition(RaceID id, RaceVisual visual, RaceAction[] actions, RaceCompatibility[] compatibilities)
        {
            ID = id;
            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";

            Visual = visual != null ? visual : RaceVisual.NONE;
            Actions = actions ?? RaceAction.NONE;
            Compatibilities = compatibilities ?? RaceCompatibility.NONE;
        }
        internal RaceDefinition(RaceEntry entry) : this(entry.ID, entry.Visual, entry.Actions, entry.Compatibilities) { }
    }
}
