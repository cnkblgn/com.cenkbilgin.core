using System;
using UnityEngine;

namespace Core.Stat
{
    [Serializable]
    public struct StatID : IEquatable<StatID>
    {
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = StatDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public StatID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public StatID(string key) : this (key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is StatID other && Equals(other);
        public readonly bool Equals(StatID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(StatID left, StatID right) => left.Equals(right);
        public static bool operator !=(StatID left, StatID right) => !left.Equals(right);

        public readonly StatDefinition GetDefinition() => StatDatabase.GetDefinition(this);
    }
}
