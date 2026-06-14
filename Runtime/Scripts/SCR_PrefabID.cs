using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public struct PrefabID : IEquatable<PrefabID>
    {
        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public PrefabID(string key) => this.key = key;
        public readonly bool Equals(PrefabID other) => key == other.key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;

        public readonly bool TryGet(out GameObject prefab)
        {
            prefab = null;
            
            if (ManagerCorePrefab.HasInstance && ManagerCorePrefab.Instance.TryGet(this, out prefab))
            {
                return true;
            }

            return false;
        }
        public readonly bool TrySpawn(Vector3 position, Quaternion rotation, Transform parent, out GameObject spawned)
        {
            spawned = null;

            if (ManagerCorePrefab.HasInstance && ManagerCorePrefab.Instance.TrySpawn(this, position, rotation, parent, out spawned))
            {
                return true;
            }

            return false;
        }
    }
}
