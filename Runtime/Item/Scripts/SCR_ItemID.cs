using System;
using UnityEngine;

namespace Core.Item
{
    using static CoreUtility;

    [Serializable]
    public partial struct ItemID : IEquatable<ItemID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public ItemID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly override bool Equals(object obj) => obj is ItemID other && Equals(other);
        public readonly bool Equals(ItemID other) => key == other.key;
        public static bool operator ==(ItemID left, ItemID right) => left.Equals(right);
        public static bool operator !=(ItemID left, ItemID right) => !left.Equals(right);

        public readonly ItemDefinition GetDefinition() => ItemDatabase.GetDefinition(this);
        public readonly ItemData CreateData() => ItemDatabase.CreateData(this);
    }
}
