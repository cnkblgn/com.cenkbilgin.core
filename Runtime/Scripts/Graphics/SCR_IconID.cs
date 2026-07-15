using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public partial struct IconID : IEquatable<IconID>
    {
        public static readonly IconID NONE = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public IconID(string key) => this.key = key;

        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(IconID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is IconID other && Equals(other);
        public static bool operator ==(IconID left, IconID right) => left.Equals(right);
        public static bool operator !=(IconID left, IconID right) => !left.Equals(right);

        public readonly Sprite Get() => IconDatabase.GetSprite(this);
    }
}
