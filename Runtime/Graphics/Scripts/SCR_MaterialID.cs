using System;
using UnityEngine;

namespace Core.Graphics
{
    [Serializable]
    public struct MaterialID : IEquatable<MaterialID>
    {
        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = MaterialDatabase.GetIDIndex(key);
                }

                return index;
            }
        }
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public MaterialID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is MaterialID other && Equals(other);
        public readonly bool Equals(MaterialID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(MaterialID left, MaterialID right) => left.Equals(right);
        public static bool operator !=(MaterialID left, MaterialID right) => !left.Equals(right);

        public readonly Material Get() => MaterialDatabase.GetMaterial(this);
    }
}
