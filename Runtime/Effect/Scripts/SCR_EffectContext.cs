namespace Core.Effect
{
    public readonly struct EffectContext
    {
        public readonly EffectState State;
        public readonly EffectInstance Instance;

        public EffectContext(EffectState state, EffectInstance instance)
        {
            State = state;
            Instance = instance;
        }
    }
}
