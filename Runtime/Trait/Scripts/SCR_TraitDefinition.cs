namespace Core.Trait
{
    public sealed class TraitDefinition
    {
        public readonly TraitID ID;
        public readonly int Cost;
        internal readonly TraitCompatibility[] Compatibilities;
        internal readonly TraitAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal TraitDefinition(TraitID id, int cost, TraitCompatibility[] compatibilities, TraitAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities ?? TraitCompatibility.NONE;
            Actions = actions ?? TraitAction.NONE;
            Cost = cost;

            IconID = $"Trait{ID.Key}";
            NameID = $"trait.{ID.Key}Name";
            DescID = $"trait.{ID.Key}Desc";
        }
        internal TraitDefinition(TraitEntry entry) : this(entry.ID, entry.Cost, entry.Compatibilities, entry.Actions) { }
    }
}