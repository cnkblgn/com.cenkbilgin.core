namespace Core.Trait
{
    public readonly struct TraitInstance
    {
        public readonly TraitID ID;

        public TraitInstance(TraitID id) : this(id.GetDefinition()) { }
        internal TraitInstance(TraitDefinition definition)
        {
            ID = definition.ID;
        }
    }
}