using System;
using UnityEngine;

namespace Core.Item
{
    [Serializable]
    public struct ItemTag : IEquatable<ItemTag>
    {
        public static readonly ItemTag GENERIC = new("GENERIC", 0);

        public readonly string Key => key;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = ItemDatabase.GetTagIndex(key);

                resolved = index >= 0;

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

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public ItemTag(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public ItemTag(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly bool Equals(ItemTag other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public readonly override bool Equals(object obj) => obj is ItemTag other && Equals(other);
        public static bool operator ==(ItemTag left, ItemTag right) => left.Equals(right);
        public static bool operator !=(ItemTag left, ItemTag right) => !left.Equals(right);

        public static ulong CreateMask(ItemTag[] tags)
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
