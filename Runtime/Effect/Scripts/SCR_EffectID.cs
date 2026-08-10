using System;
using UnityEngine;

namespace Core.Effect
{
    [Serializable]
    public struct EffectID : IEquatable<EffectID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public EffectID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is EffectID other && Equals(other);
        public readonly bool Equals(EffectID other) => index == other.index;
        public static bool operator ==(EffectID left, EffectID right) => left.Equals(right);
        public static bool operator !=(EffectID left, EffectID right) => !left.Equals(right);

        public readonly EffectDefinition GetDefinition() => EffectDatabase.GetDefinition(this);
        public readonly EffectInstance CreateInstance(float duration) => EffectDatabase.CreateInstance(this, duration);
    }
}
