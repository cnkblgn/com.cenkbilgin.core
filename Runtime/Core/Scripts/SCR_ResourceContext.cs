namespace Core
{
    public readonly struct ResourceContext
    {
        public readonly ResourceState State;
        public readonly float Current;
        public readonly float Maximum;

        public ResourceContext(ResourceState state, float current, float maximum)
        {
            State = state;
            Current = current;
            Maximum = maximum;
        }
    }
}
