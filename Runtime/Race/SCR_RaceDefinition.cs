namespace Core.Race
{
    public sealed class RaceDefinition
    {
        public readonly RaceID ID;
        public readonly RaceMask Mask;
        public readonly RaceComponent Component;
        internal readonly Compatibility[] Compatibilities;
        internal readonly RaceAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal RaceDefinition(RaceID id, RaceMask mask, RaceComponent component, Compatibility[] compatibilities, RaceAction[] actions)
        {
            ID = id;
            Mask = mask;
            Component = component != null ? component : RaceComponent.NONE;
            Actions = actions ?? RaceAction.NONE;
            Compatibilities = compatibilities ?? Compatibility.NONE;

            IconID = $"Race{ID.Key}";
            NameID = $"race.{ID.Key}Name";
            DescID = $"race.{ID.Key}Desc";
        }
        internal RaceDefinition(RaceEntry entry) : this(entry.ID, entry.Tags.CreateMask(), entry.Component, entry.Compatibilities, entry.Actions) { }
    }
}
