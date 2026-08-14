using System;
using UnityEngine;

namespace Core.Surface
{
    [Serializable]
    public struct SurfaceTag : IEquatable<SurfaceTag>
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

                index = SurfaceDatabase.GetTagIndex(key);

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

        public SurfaceTag(string key, int index)
        {
            this.key = key;
            this.index = index;
            this.resolved = true;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly bool Equals(SurfaceTag other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public readonly override bool Equals(object obj) => obj is SurfaceTag other && Equals(other);
        public static bool operator ==(SurfaceTag left, SurfaceTag right) => left.Equals(right);
        public static bool operator !=(SurfaceTag left, SurfaceTag right) => !left.Equals(right);

        public static ulong CreateMask(SurfaceTag[] tags)
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