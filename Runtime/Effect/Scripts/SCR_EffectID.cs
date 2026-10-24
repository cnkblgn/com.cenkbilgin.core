using System;
using UnityEngine;

namespace Core.Effect
{
    [Serializable]
    public struct EffectID : IEquatable<EffectID>, ISerializationCallbackReceiver
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

                index = EffectDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public EffectID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public EffectID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is EffectID other && Equals(other);
        public readonly bool Equals(EffectID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(EffectID left, EffectID right) => left.Equals(right);
        public static bool operator !=(EffectID left, EffectID right) => !left.Equals(right);
        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize() => resolved = false;

        public readonly EffectDefinition GetDefinition() => EffectDatabase.GetDefinition(this);
        public readonly EffectInstance CreateInstance(float duration) => new(this, duration);
        public readonly bool IsCompatibleWith<TContext>(TContext context)
        {
            EffectCompatibility[] compatibilities = GetDefinition().Compatibilities;

            for (int i = 0; i < compatibilities.Length; i++)
            {
                if (compatibilities[i] is EffectCompatibility<TContext> compatibility && !compatibility.IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
