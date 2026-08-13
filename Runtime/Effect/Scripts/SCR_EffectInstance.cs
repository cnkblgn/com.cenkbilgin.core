namespace Core.Effect
{
    public struct EffectInstance
    {
        public static readonly EffectInstance EMPTY = new();

        public readonly EffectID ID;
        public readonly float Duration;

        public float TimeRemaining;

        public readonly float TickInterval;
        public float TickTimer;
        public int TickCount;

        public EffectInstance(EffectDefinition definition, float duration)
        {
            ID = definition.ID;
            Duration = duration;
            TimeRemaining = duration;
            TickInterval = definition.Interval;
            TickTimer = 0;
            TickCount = 0;
        }
    }
}
