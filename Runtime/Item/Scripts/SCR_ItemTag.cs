using System;
using UnityEngine;

namespace Core.Item
{
    using static CoreUtility;

    [Serializable]
    public partial struct ItemTag : IEquatable<ItemTag>
    {
        public static readonly ItemTag GENERIC = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => IsValid ? 1UL << index : 0;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public ItemTag(string key, int index)
        {
            this.key = key;
            this.index = index;

            if (index >= 64)
            {
                Debug.LogError("Warning actor tag supports only 63 index!");
            }
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(ItemTag other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is ItemTag other && Equals(other);
        public static bool operator ==(ItemTag left, ItemTag right) => left.Equals(right);
        public static bool operator !=(ItemTag left, ItemTag right) => !left.Equals(right);

        public static ulong CreateMask(ItemTag[] tags)
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
