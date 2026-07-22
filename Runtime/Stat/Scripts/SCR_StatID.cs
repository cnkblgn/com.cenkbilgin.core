using System;
using UnityEngine;

namespace Core.Stat
{
    using static CoreUtility;

    [Serializable]
    public partial struct StatID : IEquatable<StatID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public StatID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly override bool Equals(object obj) => obj is StatID other && Equals(other);
        public readonly bool Equals(StatID other) => key == other.key;
        public static bool operator ==(StatID left, StatID right) => left.Equals(right);
        public static bool operator !=(StatID left, StatID right) => !left.Equals(right);

        public readonly StatDefinition GetDefinition() => StatDatabase.GetDefinition(this);
    }
}
