using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public sealed class ManagerCorePrefab : Manager<ManagerCorePrefab>
    {
        [Header("_")]
        [SerializeField, Required] private PrefabDatabaseConfig database = null;

        protected override void Awake()
        {
            base.Awake();

            if (database == null) throw new NullReferenceException($"[{nameof(database)}]");

            database.Build();
        }

        public bool TryGet(PrefabID id, out GameObject prefab) => database.TryGet(id, out prefab);
        public bool TrySpawn(PrefabID id, Vector3 position, Quaternion rotation, Transform parent, out GameObject gameObject)
        {
            gameObject = null;

            if (database.TryGet(id, out GameObject prefab))
            {
                gameObject = GameObject.Instantiate(prefab, position, rotation, parent);
                return true;
            }

            return false;
        }
    }
}
