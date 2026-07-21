using System;
using UnityEngine;

namespace Core.Stat
{
    using static CoreUtility;

    [Serializable]
    public partial struct StatID : IEquatable<StatID>
    {
        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public StatID(string key) => this.key = key;

        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly override bool Equals(object obj) => obj is StatID other && Equals(other);
        public readonly bool Equals(StatID other) => key == other.key;
        public static bool operator ==(StatID left, StatID right) => left.Equals(right);
        public static bool operator !=(StatID left, StatID right) => !left.Equals(right);

        public readonly StatDefinition GetDefinition() => StatDatabase.GetDefinition(this);
    }
}
