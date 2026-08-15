using System;
using UnityEngine;

namespace Core.Graphics
{
    [Serializable]
    public struct MeshID : IEquatable<MeshID>
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

                index = MeshDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public MeshID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public MeshID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is MeshID other && Equals(other);
        public readonly bool Equals(MeshID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(MeshID left, MeshID right) => left.Equals(right);
        public static bool operator !=(MeshID left, MeshID right) => !left.Equals(right);

        public readonly Mesh Get() => MeshDatabase.GetMesh(this);
    }
}
