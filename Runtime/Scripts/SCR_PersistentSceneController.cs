using System;
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
        [SerializeField, Required] private List<PersistentInstanceEntity> entityList = new();

        private readonly Dictionary<Guid, PersistentInstanceEntity> entityTable = new();     
        private PersistentSceneData thisData = new();
        private bool isLoading = false;

        private void Awake()
        {
            PersistentInstanceEntity.OnEntityMarkedForDestroy += OnEntityRequestDestroy;

            foreach (PersistentInstanceEntity entity in entityList)
            {
                if (entity == null)
                {
                    throw new Exception("PersistentSceneController.Awake() entity == null");
                }

                entityTable[entity.InstanceID] = entity;
            }

            ManagerCorePersistent.Instance.RegisterController(this);
        }
        private void OnDisable()
        {
            PersistentInstanceEntity.OnEntityMarkedForDestroy -= OnEntityRequestDestroy;

            if (ManagerCorePersistent.Instance != null)
            {
                ManagerCorePersistent.Instance.UnregisterController(this);
            }         
        }
        private void OnEntityRequestDestroy(PersistentInstanceEntity entityObject)
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
            entityList = GetComponentsInChildren<PersistentInstanceEntity>(true).ToList();
            var duplicatedGroups = entityList.GroupBy(e => e.InstanceID).Where(g => g.Count() > 1).ToList();

            foreach (var entity in entityList)
            {
                entity.GenerateID();

                UnityEditor.EditorUtility.SetDirty(entity);
            }

            foreach (var group in duplicatedGroups)
            {
                foreach (var entity in group.Skip(1))
                {
                    entity.GenerateID(true);

                    Debug.LogWarning($"PersistentSceneController.Populate() Duplicate GUID fixed: {group.Key} on object {entity.name}, new GUID: {entity.InstanceID}");

                    UnityEditor.EditorUtility.SetDirty(entity);
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.LogWarning($"PersistentSceneController.Populate() collected {entityList.Count} objects!");
        }
#endif
        public bool TryRegister(GameObject gameObject, out PersistentInstanceEntity persistentObject)
        {
            if (!gameObject.TryGetComponent(out persistentObject))
            {
                Debug.LogError($"PersistentSceneController.RegisterController() [{gameObject}] has no [PersistentInstanceEntity]");
                Destroy(gameObject);
                return false;
            }

            persistentObject.GenerateID();
            Register(persistentObject);

            return true;
        }
        private void Register(PersistentInstanceEntity entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            if (entityTable.ContainsKey(entityObject.InstanceID))
            {
                Debug.LogError($"PersistentSceneController.Register() Trying to duplicate {entityObject.InstanceID}");
                return;
            }

            entityList.Add(entityObject);
            entityTable[entityObject.InstanceID] = entityObject;
        }
        private void Unregister(PersistentInstanceEntity entityObject)
        {
            if (entityObject == null)
            {
                return;
            }

            if (!entityTable.ContainsKey(entityObject.InstanceID))
            {
                Debug.LogError($"PersistentSceneController.Unregister() Trying to unregister twice {entityObject.InstanceID}");
                return;
            }

            entityList.Remove(entityObject);
            entityTable.Remove(entityObject.InstanceID);
        }

        public PersistentSceneData Export()
        {
            PersistentSceneData sceneData = new(ManagerCoreGame.Instance.GetCurrentScene(), new());

            foreach (var item in thisData.Database)
            {
                Guid id = item.Key;
                PersistentInstanceData data = item.Value;

                if (data.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.Database[id] = new(data.TypeID, data.PrefabID, data.InstanceID, data.IsMarkedForDestroy, data.Data);
            }

            foreach (PersistentInstanceEntity entityObject in entityList)
            {
                if (entityObject.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.Database[entityObject.InstanceID] = entityObject.Export();
            }

            return sceneData;
        }
        public void Import(PersistentSceneData sceneData)
        {
            isLoading = true;

            thisData = new(sceneData.ID, sceneData.Database);

            // Iterate snapshot to allow safe removal
            foreach (PersistentInstanceEntity entityObject in new List<PersistentInstanceEntity>(entityList))
            {
                // If data not in database
                if (!thisData.Database.TryGetValue(entityObject.InstanceID, out var incomingData))
                {
                    entityObject.MarkForDestroy();
                    continue;
                }

                // If data not in database or expilicitly marked as destroyed
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
            foreach (var remainingData in thisData.Database)
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
                    if (gameObject.TryGetComponent(out PersistentInstanceEntity entityObject))
                    {
                        entityObject.Import(remainingData.Value);
                        Register(entityObject);
                    }
                    else
                    {
                        Debug.LogWarning($"PersistentSceneController.Import() [PersistentInstanceEntity] component not found in [{gameObject.name}]");
                    }
                }
                else
                {
                    Debug.LogWarning($"PersistentSceneController.Import() Prefab not found for Orphan data: [{remainingData.Value.PrefabID}]");
                }
            }

            entityTable.Clear();

            foreach (PersistentInstanceEntity entity in entityList)
            {
                if (entity != null)
                {
                    entityTable[entity.InstanceID] = entity;
                }
            }

            isLoading = false;
        }
    }
}