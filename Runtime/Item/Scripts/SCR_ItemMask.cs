using System;

namespace Core.Item
{
    public readonly struct ItemMask : IEquatable<ItemMask>
    {
        public static readonly ItemMask ALL = new(ulong.MaxValue);
        public static readonly ItemMask NONE = new(0UL);

        public bool IsValid => Value != 0;

        public readonly ulong Value;

        public ItemMask(ulong value) => Value = value;

        public bool HasAll(ItemMask target) => (Value & target.Value) == target.Value;
        public bool HasAll(ItemTag[] target) => HasAll(target.CreateMask());
        public bool HasAny(ItemMask target) => (Value & target.Value) != 0;
        public bool HasAny(ItemTag target) => HasAny(target.Mask);
        public bool HasAny(ItemTag[] target) => HasAny(target.CreateMask());
        public bool Equals(ItemMask other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ItemMask other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static ItemMask operator ~(ItemMask mask) => new(~mask.Value);
        public static bool operator ==(ItemMask left, ItemMask right) => left.Equals(right);
        public static bool operator !=(ItemMask left, ItemMask right) => !left.Equals(right);
    }
}
