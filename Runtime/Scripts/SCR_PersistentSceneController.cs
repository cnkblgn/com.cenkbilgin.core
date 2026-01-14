using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class PersistentSceneController : MonoBehaviour
    {
        public bool IsLoading => isLoading;

        [Header("_")]
        [SerializeField, Required] private List<PersistentInstanceObject> entityList = new();

        private readonly Dictionary<string, PersistentInstanceObject> entityTable = new();
        private PersistentIDGenerator thisGenerator = new(0, "entity");
        private PersistentSceneData thisData = new();
        private bool isLoading = false;

        private void Awake()
        {
            PersistentInstanceObject.OnMarkedForDestroy += OnRequestDestroy;

            thisGenerator.Sync(entityList.Select(e => e.InstanceID));

            foreach (PersistentInstanceObject entity in entityList)
            {
                if (entity == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(entity.InstanceID))
                {
                    entity.SetID(thisGenerator.Generate().ToString());
                }

                entityTable[entity.InstanceID] = entity;
            }

            ManagerCorePersistent.Instance.RegisterController(this);
        }
        private void OnDisable()
        {
            PersistentInstanceObject.OnMarkedForDestroy -= OnRequestDestroy;

            if (ManagerCorePersistent.Instance != null)
            {
                ManagerCorePersistent.Instance.UnregisterController(this);
            }         
        }
        private void OnRequestDestroy(PersistentInstanceObject entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            if (!entityObject.IsMarkedForDestroy)
            {
                return;
            }

            if (thisData.Database.TryGetValue(entityObject.InstanceID, out var data))
            {
                data.IsMarkedForDestroy = true;
            }

            Unregister(entityObject);
            Destroy(entityObject.gameObject);
        }

#if UNITY_EDITOR
        public void Populate()
        {
            entityList = GetComponentsInChildren<PersistentInstanceObject>(true).ToList();
            Debug.LogWarning($"PersistentSceneController.Populate() collected {entityList.Count} objects!");
        }
#endif

        public bool TryRegister(GameObject gameObject, out PersistentInstanceObject persistentObject)
        {
            if (!gameObject.TryGetComponent(out persistentObject))
            {
                Debug.LogError($"PersistentSceneController.RegisterController() [{gameObject}] has no [PersistentInstanceObject]");
                Destroy(gameObject);
                return false;
            }

            persistentObject.SetID(thisGenerator.Generate().ToString());
            Register(persistentObject);

            return true;
        }
        private void Register(PersistentInstanceObject entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(entityObject.InstanceID))
            {
                Debug.LogError($"PersistentSceneController.RegisterController() Trying to register {entityObject.gameObject} without InstanceID");
                return;
            }

            if (entityTable.ContainsKey(entityObject.InstanceID))
            {
                Debug.LogError($"PersistentSceneController.RegisterController() Trying to duplicate {entityObject.InstanceID}");
                return;
            }

            entityList.Add(entityObject);
            entityTable[entityObject.InstanceID] = entityObject;
        }
        private void Unregister(PersistentInstanceObject entityObject)
        {
            if (entityObject == null || !entityTable.ContainsKey(entityObject.InstanceID))
            {
                return;
            }

            entityList.Remove(entityObject);
            entityTable.Remove(entityObject.InstanceID);
        }

        public void ExportTo(ref PersistentSceneData sceneData)
        {
            sceneData.Database.Clear();
            sceneData.ID = ManagerCoreGame.Instance.GetCurrentScene();

            foreach (var item in thisData.Database)
            {
                if (item.Value.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.Database[item.Key] = new(item.Value.TypeID, item.Value.PrefabID, item.Value.InstanceID, item.Value.IsMarkedForDestroy, item.Value.Data);
            }

            foreach (PersistentInstanceObject entityObject in entityList)
            {
                if (entityObject.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.Database[entityObject.InstanceID] = entityObject.Export();
            }

            sceneData.Generator = new(thisGenerator.NextID, thisGenerator.Prefix);
        }
        public void ImportFrom(ref PersistentSceneData sceneData)
        {
            isLoading = true;

            thisData = new(sceneData.ID, sceneData.Database, sceneData.Generator);

            thisGenerator = thisData.Generator;

            Dictionary<string, PersistentInstanceData> dataLookup = sceneData.Database;

            // Iterate snapshot to allow safe removal
            foreach (PersistentInstanceObject entityObject in new List<PersistentInstanceObject>(entityList))
            {
                // If data not in database => destroy and unregister
                if (!dataLookup.TryGetValue(entityObject.InstanceID, out var incomingData))
                {
                    entityObject.MarkForDestroy();
                    Unregister(entityObject);
                    Destroy(entityObject.gameObject);
                    continue;
                }

                // If explicitly marked for destroy => destroy and unregister
                if (incomingData.IsMarkedForDestroy)
                {
                    Unregister(entityObject);
                    Destroy(entityObject.gameObject);
                    continue;
                }

                // Otherwise update existing object
                entityObject.Import(incomingData);
            }

            // Initialize any newly spawned objects, eg. Orphan datas
            foreach (var remainingData in dataLookup)
            {
                if (remainingData.Value.IsMarkedForDestroy)
                {
                    continue;
                }

                if (entityTable.ContainsKey(remainingData.Key))
                {
                    continue;
                }

                if (ManagerCorePrefab.Instance.TrySpawn(remainingData.Value.PrefabID, Vector3.zero, Quaternion.identity, null, out GameObject gameObject))
                {
                    if (gameObject.TryGetComponent(out PersistentInstanceObject entityObject))
                    {
                        entityObject.Import(remainingData.Value);
                        Register(entityObject);
                    }
                }
                else
                {
                    Debug.LogWarning($"PersistentSceneController.ImportFrom() Prefab not found for Orphan data: [{remainingData.Value.PrefabID}]");
                }
            }

            entityTable.Clear();
            foreach (PersistentInstanceObject entity in entityList)
            {
                if (entity != null)
                {
                    entityTable[entity.InstanceID] = entity;
                }
            }

            thisGenerator.Sync(entityList.Select(e => e.InstanceID));

            isLoading = false;
        }
    }
}