using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "SCO_PrefabDatabase", menuName = "Resources/Prefab Database Config", order = 0)]
    public class PrefabDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField] private GameObject[] collection = null;

        private Dictionary<string, GameObject> table = new();

        public bool TryGet(string id, out GameObject prefab)
        {
            if (id == null)
            {
                throw new ArgumentNullException($"PrefabDatabaseConfig.TryGet() [{nameof(id)}]");
            }

            if (table.TryGetValue(id, out prefab))
            {
                return true;
            }

            Debug.LogError($"PrefabDatabaseConfig.TryGet() prefab not found! [{id}]");
            return false;
        }

        public IReadOnlyCollection<string> Get() => table.Keys;

        public void Build()
        {
            table = new Dictionary<string, GameObject>(collection.Length);

            for (int i = 0; i < collection.Length; i++)
            {
                GameObject prefab = collection[i];

                if (prefab == null)
                {
                    Debug.LogError($"PrefabDatabaseConfig.Build() prefab == null at [{i}]");
                    continue;
                }

                string id = prefab.name;

                if (table.ContainsKey(id))
                {
                    Debug.LogError($"PrefabDatabaseConfig.Build() duplicate prefab [{i}]");
                    continue;
                }

                table.Add(id, prefab);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            try
            {
                Build();
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }
#endif
    }
}
