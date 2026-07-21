namespace Core.Stat
{
    public readonly struct StatModifier
    {
        public static readonly StatModifier Empty = new();

        public readonly StatID StatID;
        public readonly float Value;

        public readonly StatModifierOperation Operation;
        public readonly StatModifierSource Source;

        public StatModifier(StatID statID, float value, StatModifierOperation operation, StatModifierSource source)
        {
            StatID = statID;
            Value = value;
            Operation = operation;
            Source = source;
        }
    }
}
