using System;
using UnityEngine;

namespace Core.Damage
{
    [Serializable]
    public partial struct DamageTag : IEquatable<DamageTag>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => IsValid ? 1UL << index : 0;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public DamageTag(string key, int index)
        {
            this.key = key;
            this.index = (int)index;

            if (index >= 64)
            {
                Debug.LogWarning("Warning damage tag supports only 63 index!");
            }
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly bool Equals(DamageTag other) => index == other.index;
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