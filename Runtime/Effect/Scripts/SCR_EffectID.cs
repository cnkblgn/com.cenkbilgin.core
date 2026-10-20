using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Effect
{
    [Serializable]
    public struct EffectID : IEquatable<EffectID>
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

        public readonly EffectDefinition GetDefinition() => EffectDatabase.GetDefinition(this);
        public readonly EffectInstance CreateInstance(float duration) => EffectDatabase.CreateInstance(this, duration);
        public readonly bool IsInCompatibleWith(IReadOnlyList<EffectInstance> traits, GameObject actor)
        {
            EffectValidator[] validators = GetDefinition().Validators;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsInCompatibleWith(this, traits, actor))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsInCompatibleWith(EffectID id)
        {
            EffectValidator[] validators = GetDefinition().Validators;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsInCompatibleWith(this, id))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
