using System;
using UnityEngine;

namespace Core.Prefab
{
    using static CoreUtility;

    [Serializable]
    public partial struct PrefabID : IEquatable<PrefabID>
    {
        public static readonly PrefabID NONE = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public PrefabID(string key) => this.key = key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(PrefabID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is PrefabID other && Equals(other);
        public static bool operator ==(PrefabID left, PrefabID right) => left.Equals(right);
        public static bool operator !=(PrefabID left, PrefabID right) => !left.Equals(right);

        public readonly GameObject Get() => PrefabDatabase.GetPrefab(this);
        public readonly GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent) => PrefabDatabase.SpawnPrefab(this, position, rotation, parent);
    }
}
