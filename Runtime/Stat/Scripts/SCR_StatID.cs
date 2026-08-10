using System;
using UnityEngine;

namespace Core.Stat
{
    [Serializable]
    public struct StatID : IEquatable<StatID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public StatID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is StatID other && Equals(other);
        public readonly bool Equals(StatID other) => index == other.index;
        public static bool operator ==(StatID left, StatID right) => left.Equals(right);
        public static bool operator !=(StatID left, StatID right) => !left.Equals(right);

        public readonly StatDefinition GetDefinition() => StatDatabase.GetDefinition(this);
    }
}
