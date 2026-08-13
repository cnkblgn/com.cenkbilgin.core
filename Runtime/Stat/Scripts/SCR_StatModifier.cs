namespace Core.Stat
{
    using static CoreUtility;

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

        private readonly bool IsPositive(StatDefinition definition)
        {
            bool increase = Operation == StatModifierOperation.MULTIPLY ? Value >= 1f : Value >= 0f;

            return definition.Tag == StatTag.POSITIVE ? increase : definition.Tag == StatTag.NEGATIVE && !increase;
        }
        public readonly string GetDescription()
        {
            StatDefinition definition = StatID.GetDefinition();

            string operation = Operation == StatModifierOperation.MULTIPLY ? "x" : Value >= 0 ? "+" : STRING_EMPTY;

            operation = IsPositive(definition) ? operation.ToGreen() : operation.ToRed();

            return $"{definition.NameID.Get()} {operation}{Value:0.00}";
        }
    }
}
