namespace Core.Stat
{
    public readonly struct StatContext
    {
        public readonly StatState State;
        public readonly StatModifier Modifier;

        public StatContext(StatState state, StatModifier modifier)
        {
            State = state;
            Modifier = modifier;
        }
    }
}