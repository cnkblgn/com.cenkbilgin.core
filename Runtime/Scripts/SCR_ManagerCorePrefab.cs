using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public class ManagerCorePrefab : Manager<ManagerCorePrefab>
    {
        [Header("_")]
        [SerializeField, Required] private PrefabDatabaseConfig database = null;

        private readonly Dictionary<string, GameObject> thisTable = new();

        private void Start()
        {
            if (database == null)
            {
                LogWarning("ManagerCorePrefab.Start() database == null");
                return;
            }

            PrefabData[] value = database.Collection;

            foreach (PrefabData i in value)
            {
                thisTable.Add(i.ID, i.Object);
            }
        }
        public bool TrySpawn(string id, Vector3 position, Quaternion rotation, Transform parent, out GameObject gameObject)
        {
            gameObject = null;

            if (!TryGet(id, out GameObject prefab))
            {
                return false;
            }

            gameObject = GameObject.Instantiate(prefab, position, rotation, parent);

            return true;
        }
        private bool TryGet(string id, out GameObject prefab)
        {
            if (id == null)
            {
                prefab = null;
                return false;
            }

            return thisTable.TryGetValue(id, out prefab);
        }
        public bool Contains(string id)
        {
            if (id == null)
            {
                LogError("ManagerCorePrefab.Contains() id == null");
                return false;
            }

            return thisTable.ContainsKey(id);
        }
    }
}
