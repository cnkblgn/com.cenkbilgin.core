using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public struct IconID : IEquatable<IconID>
    {
        public static readonly IconID NONE = new(STRING_EMPTY, -1);

        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = IconDatabase.GetIDIndex(key);
                }

                return index;
            }
        }
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public IconID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is IconID other && Equals(other);
        public readonly bool Equals(IconID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(IconID left, IconID right) => left.Equals(right);
        public static bool operator !=(IconID left, IconID right) => !left.Equals(right);

        public readonly Sprite GetSprite() => IconDatabase.GetSprite(this);
    }
}
