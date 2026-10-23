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
        public readonly EffectInstance CreateInstance(float duration) => new(this, duration);
        internal readonly bool IsCompatibleWith(IReadOnlyList<EffectInstance> effects)
        {
            if (effects == null)
            {
                Debug.LogError("Effect compatibility check failed! effect is null!?");
                return false;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (!IsCompatibleWith(effects[i]))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(EffectID id)
        {
            if (id.IsValid)
            {
                return false;
            }

            EffectCompatibility[] validators = GetDefinition().Compatibilities;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsCompatibleWith(this, id))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(object context)
        {
            if (context == null)
            {
                Debug.LogError("Trait compatibility check failed! context is null!?");
                return false;
            }

            EffectCompatibility[] validators = GetDefinition().Compatibilities;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(int context)
        {
            EffectCompatibility[] validators = GetDefinition().Compatibilities;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
        public readonly bool IsCompatibleWith(ulong context)
        {
            EffectCompatibility[] validators = GetDefinition().Compatibilities;

            for (int i = 0; i < validators.Length; i++)
            {
                if (!validators[i].IsCompatibleWith(this, context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
