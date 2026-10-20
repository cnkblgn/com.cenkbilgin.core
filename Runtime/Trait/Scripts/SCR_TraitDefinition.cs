namespace Core.Trait
{
    public sealed class TraitDefinition
    {
        public readonly TraitID ID;
        public readonly int Cost;
        internal readonly TraitValidator[] Validators;
        internal readonly TraitAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal TraitDefinition(TraitID id, int cost, TraitValidator[] validator, TraitAction[] actions)
        {
            ID = id;
            Validators = validator ?? TraitValidator.NONE;
            Actions = actions ?? TraitAction.NONE;
            Cost = cost;

            IconID = $"ICON_{ID.Key}";
            NameID = $"trait.{ID.Key}.name";
            DescID = $"trait.{ID.Key}.desc";
        }
        internal TraitDefinition(TraitEntry entry) : this(entry.ID, entry.Cost, entry.Validators, entry.Actions) { }
    }
}