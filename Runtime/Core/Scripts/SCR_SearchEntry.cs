namespace Core
{
    public readonly struct SearchEntry<T>
    {
        public readonly string Label;
        public readonly T Value;

        public SearchEntry(string label, T value)
        {
            Label = label;
            Value = value;
        }
    }
}
