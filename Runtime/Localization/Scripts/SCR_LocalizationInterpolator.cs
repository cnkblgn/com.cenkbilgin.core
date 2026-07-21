using System;
using System.Text;

namespace Core.Localization
{
    [Serializable]
    public abstract class LocalizationInterpolator
    {
        public abstract string Key { get; }

        internal bool TryInterpolate(string text, int valueStart, int valueLength, StringBuilder builder) => TryInterpolate(text.AsSpan(valueStart, valueLength), builder);

        protected abstract bool TryInterpolate(ReadOnlySpan<char> value, StringBuilder builder);
    }
}