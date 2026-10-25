namespace Core.Trait
{
    public sealed class TraitDefinition
    {
        public readonly TraitID ID;
        public readonly int Cost;
        internal readonly TraitAction[] Actions;
        internal readonly Compatibility[] Compatibilities;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal TraitDefinition(TraitID id, int cost, Compatibility[] compatibilities, TraitAction[] actions)
        {
            ID = id;
            Compatibilities = compatibilities ?? Compatibility.NONE;
            Actions = actions ?? TraitAction.NONE;
            Cost = cost;

            IconID = $"Trait{ID.Key}";
            NameID = $"trait.{ID.Key}Name";
            DescID = $"trait.{ID.Key}Desc";
        }
        internal TraitDefinition(TraitEntry entry) : this(entry.ID, entry.Cost, entry.Compatibilities, entry.Actions) { }
    }
}