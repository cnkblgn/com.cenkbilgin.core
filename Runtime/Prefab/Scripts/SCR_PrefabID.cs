using System;
using UnityEngine;

namespace Core.Prefab
{
    [Serializable]
    public struct PrefabID : IEquatable<PrefabID>
    {
        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = PrefabDatabase.GetIDIndex(key);
                }

                return index;
            }
        }
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public PrefabID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is PrefabID other && Equals(other);
        public readonly bool Equals(PrefabID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(PrefabID left, PrefabID right) => left.Equals(right);
        public static bool operator !=(PrefabID left, PrefabID right) => !left.Equals(right);

        public readonly GameObject Get() => PrefabDatabase.GetPrefab(this);
        public readonly GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent) => PrefabDatabase.SpawnPrefab(this, position, rotation, parent);
    }
}
