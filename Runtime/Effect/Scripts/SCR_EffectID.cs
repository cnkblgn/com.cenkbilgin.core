using System;
using UnityEngine;

namespace Core.Effect
{
    using static CoreUtility;

    [Serializable]
    public partial struct EffectID : IEquatable<EffectID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public EffectID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => HashCode.Combine(key, index);
        public readonly override bool Equals(object obj) => obj is EffectID other && Equals(other);
        public readonly bool Equals(EffectID other) => key == other.key && index == other.index;
        public static bool operator ==(EffectID left, EffectID right) => left.Equals(right);
        public static bool operator !=(EffectID left, EffectID right) => !left.Equals(right);

        public readonly EffectDefinition GetDefinition() => EffectDatabase.GetDefinition(this);
        public readonly EffectInstance CreateInstance(float duration) => EffectDatabase.CreateInstance(this, duration);
    }
}
