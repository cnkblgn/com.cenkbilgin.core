using System;
using UnityEngine;

namespace Core.Event
{
    [Serializable]
    public struct EventID : IEquatable<EventID>
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

                index = EventDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public EventID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public EventID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is EventID other && Equals(other);
        public readonly bool Equals(EventID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(EventID left, EventID right) => left.Equals(right);
        public static bool operator !=(EventID left, EventID right) => !left.Equals(right);
    }
}
