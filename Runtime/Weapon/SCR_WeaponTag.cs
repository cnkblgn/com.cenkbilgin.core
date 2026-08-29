using System;
using UnityEngine;

namespace Core.Weapon
{
    [Serializable]
    public struct WeaponTag : IEquatable<WeaponTag>
    {
        public readonly string Key => key;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = WeaponDatabase.GetTagIndex(key);

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

        public WeaponTag(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public WeaponTag(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly bool Equals(WeaponTag other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public readonly override bool Equals(object obj) => obj is WeaponTag other && Equals(other);
        public static bool operator ==(WeaponTag left, WeaponTag right) => left.Equals(right);
        public static bool operator !=(WeaponTag left, WeaponTag right) => !left.Equals(right);

        public static ulong CreateMask(WeaponTag[] tags)
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
