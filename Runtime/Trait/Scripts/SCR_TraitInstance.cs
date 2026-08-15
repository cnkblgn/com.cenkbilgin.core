namespace Core.Trait
{
    public readonly struct TraitInstance
    {
        public readonly TraitID ID;

        public TraitInstance(TraitDefinition definition)
        {
            ID = definition.ID;
        }
    }
}