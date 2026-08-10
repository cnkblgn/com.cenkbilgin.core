using System;
using UnityEngine;

namespace Core.Item
{
    [Serializable]
    public struct ItemID : IEquatable<ItemID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public ItemID(string key, uint index)
        {
            this.key = key;
            this.index = (int)index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is ItemID other && Equals(other);
        public readonly bool Equals(ItemID other) => index == other.index;
        public static bool operator ==(ItemID left, ItemID right) => left.Equals(right);
        public static bool operator !=(ItemID left, ItemID right) => !left.Equals(right);

        public readonly ItemDefinition GetDefinition() => ItemDatabase.GetDefinition(this);
        public readonly ItemData CreateData() => new(this);
    }
}
