using System;
using UnityEngine;

namespace Core.Graphics
{
    [Serializable]
    public struct MaterialID : IEquatable<MaterialID>
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

                index = MaterialDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public MaterialID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public MaterialID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is MaterialID other && Equals(other);
        public readonly bool Equals(MaterialID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(MaterialID left, MaterialID right) => left.Equals(right);
        public static bool operator !=(MaterialID left, MaterialID right) => !left.Equals(right);

        public readonly Material Get() => MaterialDatabase.GetMaterial(this);
    }
}
