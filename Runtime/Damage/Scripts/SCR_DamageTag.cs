using System;
using UnityEngine;

namespace Core.Damage
{
    using static CoreUtility;

    [Serializable]
    public partial struct DamageTag : IEquatable<DamageTag>
    {
        public static readonly DamageTag NONE = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => IsValid ? 1UL << index : 0;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public DamageTag(string key, int index)
        {
            this.key = key;
            this.index = index;

            if (index >= 64)
            {
                Debug.LogWarning("Warning damage tag supports only 63 index!");
            }
        }
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(DamageTag other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is DamageTag other && Equals(other);
        public static bool operator ==(DamageTag left, DamageTag right) => left.Equals(right);
        public static bool operator !=(DamageTag left, DamageTag right) => !left.Equals(right);

        public static ulong CreateMask(DamageTag[] tags)
        {
            ulong mask = 0;

            for (int i = 0; i < tags.Length; i++)
            {
                mask |= tags[i].Mask;
            }

            return mask;
        }
    }
}