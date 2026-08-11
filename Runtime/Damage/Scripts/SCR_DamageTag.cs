using System;
using UnityEngine;

namespace Core.Damage
{
    [Serializable]
    public struct DamageTag : IEquatable<DamageTag>
    {
        public readonly string Key => key;

        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = DamageDatabase.GetTagIndex(key);
                }

                return index;
            }
        }
        public ulong Mask
        {
            get
            {
                int value = Index;

                return value >= 0 && value < 64 ? 1UL << value : 0;
            }
        }
        public bool IsValid
        {
            get
            {
                int value = Index;

                return !string.IsNullOrEmpty(key) && value >= 0 && value < 64;
            }
        }

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public DamageTag(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly bool Equals(DamageTag other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public readonly override bool Equals(object obj) => obj is DamageTag other && Equals(other);
        public static bool operator ==(DamageTag left, DamageTag right) => left.Equals(right);
        public static bool operator !=(DamageTag left, DamageTag right) => !left.Equals(right);

        public static ulong CreateMask(DamageTag[] tags)
        {
            ulong mask = 0;

            if (tags == null)
            {
                return mask;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                mask |= tags[i].Mask;
            }

            return mask;
        }
    }
}