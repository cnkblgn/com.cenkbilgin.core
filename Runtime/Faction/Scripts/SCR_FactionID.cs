using System;
using UnityEngine;

namespace Core.Faction
{
    [Serializable]
    public struct FactionID : IEquatable<FactionID>
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

                index = FactionDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public FactionID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public FactionID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is FactionID other && Equals(other);
        public readonly bool Equals(FactionID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(FactionID left, FactionID right) => left.Equals(right);
        public static bool operator !=(FactionID left, FactionID right) => !left.Equals(right);

        public readonly FactionDefinition GetDefinition() => FactionDatabase.GetDefinition(this);
    }
}
