using System;
using System.Text;
using UnityEngine;

namespace Core.Localization
{
    public abstract class LocalizationInterpolator : ScriptableObject
    {
        public abstract string Key { get; }

        internal bool TryInterpolate(string text, int valueStart, int valueLength, StringBuilder builder) => TryInterpolate(text.AsSpan(valueStart, valueLength), builder);
        protected abstract bool TryInterpolate(ReadOnlySpan<char> value, StringBuilder builder);
    }
}