using System;
using UnityEngine;

namespace Core.Trait
{
    [Serializable]
    public struct TraitID : IEquatable<TraitID>, ISerializationCallbackReceiver
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

                index = TraitDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public TraitID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public TraitID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is TraitID other && Equals(other);
        public readonly bool Equals(TraitID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(TraitID left, TraitID right) => left.Equals(right);
        public static bool operator !=(TraitID left, TraitID right) => !left.Equals(right);
        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => resolved = false;

        public readonly TraitDefinition GetDefinition() => TraitDatabase.GetDefinition(this);
        public readonly TraitInstance CreateInstance() => new(this);
        public readonly bool IsCompatibleWith<TContext>(TContext context)
        {
            TraitCompatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (compatibilities[i] is TraitCompatibility<TContext> compatibility && !compatibility.IsCompatibleWith(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
