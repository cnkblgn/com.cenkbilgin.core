using System;
using UnityEngine;

namespace Core.Race
{
    [Serializable]
    public struct RaceID : IEquatable<RaceID>, ISerializationCallbackReceiver
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

                index = RaceDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public RaceID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public RaceID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is RaceID other && Equals(other);
        public readonly bool Equals(RaceID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(RaceID left, RaceID right) => left.Equals(right);
        public static bool operator !=(RaceID left, RaceID right) => !left.Equals(right);
        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => resolved = false;

        public readonly RaceDefinition GetDefinition() => RaceDatabase.GetDefinition(this);
        public readonly bool IsCompatibleWith<TContext>(TContext context)
        {
            Compatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (compatibilities[i] is Compatibility<TContext> compatibility && !compatibility.IsCompatibleWith(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
