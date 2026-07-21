namespace Core.Effect
{
    public struct EffectInstance
    {
        public static readonly EffectInstance Empty = new();

        public readonly EffectID ID;
        public readonly float Duration;

        public float TimeRemaining;

        public readonly float TickInterval;
        public float TickTimer;
        public int TickCount;

        public EffectInstance(EffectID id, float duration)
        {
            ID = id;
            Duration = duration;
            TimeRemaining = duration;
            TickInterval = id.GetDefinition().Interval;
            TickTimer = 0;
            TickCount = 0;
        }
    }
}
