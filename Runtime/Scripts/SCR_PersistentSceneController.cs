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
        private readonly HashSet<Guid> entityHashset = new();
        private bool isLoading = false;

        private void Awake()
        {
            PersistentInstanceEntity.OnMarkedForDestroy += OnEntityRequestDestroy;

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
            PersistentInstanceEntity.OnMarkedForDestroy -= OnEntityRequestDestroy;

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

            if (!entityHashset.Contains(entityObject.InstanceID))
            {
                entityHashset.Add(entityObject.InstanceID);
            }

            TryUnregister(entityObject);
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
            TryRegister(persistentObject);

            return true;
        }
        public bool TryRegister(PersistentInstanceEntity entityObject)
        {
            if (entityObject == null)
            {
                return false;
            }

            if (entityTable.ContainsKey(entityObject.InstanceID))
            {
                Debug.LogError($"PersistentSceneController.Register() Trying to duplicate {entityObject.InstanceID}");
                return false;
            }

            entityList.Add(entityObject);
            entityTable[entityObject.InstanceID] = entityObject;
            return true;
        }
        public bool TryUnregister(PersistentInstanceEntity entityObject)
        {
            if (entityObject == null)
            {
                return false;
            }

            if (!entityTable.ContainsKey(entityObject.InstanceID))
            {
                return false;
            }

            entityList.Remove(entityObject);
            entityTable.Remove(entityObject.InstanceID);
            return true;
        }

        public PersistentSceneData Export()
        {
            PersistentSceneData sceneData = new(ManagerCoreGame.Instance.GetCurrentScene(), new(), new());

            foreach (PersistentInstanceEntity entityObject in entityList)
            {
                if (entityObject == null || entityObject.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.Database[entityObject.InstanceID] = entityObject.Export();
            }

            foreach (Guid id in entityHashset)
            {
                sceneData.Hashset.Add(id);
            }

            return sceneData;
        }
        public void Import(PersistentSceneData sceneData)
        {
            isLoading = true;

            entityHashset.Clear();
            entityHashset.UnionWith(sceneData.Hashset);

            // Iterate snapshot to allow safe removal
            foreach (PersistentInstanceEntity entityObject in new List<PersistentInstanceEntity>(entityList))
            {
                Guid id = entityObject.InstanceID;

                // Silinmişse
                if (entityHashset.Contains(id))
                {
                    entityObject.MarkForDestroy(true);
                    TryUnregister(entityObject);
                    Destroy(entityObject.gameObject);
                    continue;
                }

                // Save'de varsa yükle
                if (sceneData.Database.TryGetValue(id, out var incomingData))
                {
                    entityObject.Import(incomingData);
                }
                else { } // Save'de yoksa → yeni versiyonda eklenmiş obje
            }

            // Save’de var ama sahnede yok → dinamik spawn
            foreach (var remainingData in sceneData.Database)
            {
                if (entityTable.ContainsKey(remainingData.Key))
                {
                    continue;
                }

                if (ManagerCorePrefab.Instance.TrySpawn(remainingData.Value.PrefabID, Vector3.zero, Quaternion.identity, null, out GameObject gameObject))
                {
                    if (gameObject.TryGetComponent(out PersistentInstanceEntity entityObject))
                    {
                        entityObject.Import(remainingData.Value);
                        TryRegister(entityObject);
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

            // re-populate entity table
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