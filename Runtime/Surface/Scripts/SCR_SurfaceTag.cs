using System;
using UnityEngine;

namespace Core.Surface
{
    using static CoreUtility;

    [Serializable]
    public partial struct SurfaceTag : IEquatable<SurfaceTag>
    {
        public static readonly SurfaceTag DEFAULT = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => IsValid ? 1UL << index : 0;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public SurfaceTag(string key, int index)
        {
            this.key = key;
            this.index = index;

            if (index >= 64)
            {
                Debug.LogError("Warning surface tag supports only 63 index!");
            }
        }

        public readonly override int GetHashCode() => HashCode.Combine(key, index);
        public readonly bool Equals(SurfaceTag other) => key == other.key && index == other.index;
        public readonly override bool Equals(object obj) => obj is SurfaceTag other && Equals(other);

        public static bool operator ==(SurfaceTag left, SurfaceTag right) => left.Equals(right);
        public static bool operator !=(SurfaceTag left, SurfaceTag right) => !left.Equals(right);

        public static ulong CreateMask(SurfaceTag[] tags)
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