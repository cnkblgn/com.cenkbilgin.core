using System;
using UnityEngine;

namespace Core.Effect
{
    using static CoreUtility;

    [Serializable]
    public partial struct EffectID : IEquatable<EffectID>
    {
        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public EffectID(string key) => this.key = key;

        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(EffectID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is EffectID other && Equals(other);
        public static bool operator ==(EffectID left, EffectID right) => left.Equals(right);
        public static bool operator !=(EffectID left, EffectID right) => !left.Equals(right);

        public readonly EffectDefinition GetDefinition() => EffectDatabase.GetDefinition(this);
    }
}
