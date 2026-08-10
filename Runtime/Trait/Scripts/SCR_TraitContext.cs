namespace Core.Trait
{
    public readonly struct TraitContext
    {
        public readonly TraitState State;
        public readonly TraitInstance Instance;

        public TraitContext(TraitState state, TraitInstance instance)
        {
            State = state;
            Instance = instance;
        }
    }
}