using System;
using UnityEngine;

namespace Core.Graphics
{
    [Serializable]
    public struct IconID : IEquatable<IconID>
    {
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = IconDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public IconID(string key, int index)
        {
            this.key = key;
            this.index = index;
            this.resolved= true;
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
