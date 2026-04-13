using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public class ManagerCorePrefab : Manager<ManagerCorePrefab>
    {
        [Header("_")]
        [SerializeField, Required] private PrefabDatabaseConfig database = null;

        protected override void Awake()
        {
            base.Awake();

            if (database == null)
            {
                throw new NullReferenceException($"ManagerCorePrefab.Awake() [{nameof(database)}]");
            }
        }

        public bool TrySpawn(string id, Vector3 position, Quaternion rotation, Transform parent, out GameObject gameObject)
        {
            gameObject = null;

            if (database.TryGet(id, out GameObject prefab))
            {
                gameObject = GameObject.Instantiate(prefab, position, rotation, parent);
                return true;
            }

            return false;
        }
        public IReadOnlyCollection<string> Get() => database.Get();
    }
}
