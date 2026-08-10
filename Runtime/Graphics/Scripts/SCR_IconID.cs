using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public partial struct IconID : IEquatable<IconID>
    {
        public static readonly IconID NONE = new(STRING_EMPTY, -1);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public IconID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly bool Equals(IconID other) => index == other.index;
        public readonly override bool Equals(object obj) => obj is IconID other && Equals(other);
        public static bool operator ==(IconID left, IconID right) => left.Equals(right);
        public static bool operator !=(IconID left, IconID right) => !left.Equals(right);

        public readonly Sprite GetSprite() => IconDatabase.GetSprite(this);
    }
}
