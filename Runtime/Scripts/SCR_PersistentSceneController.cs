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
        [SerializeField, Required] private List<PersistentEntity> entityList = new();

        private readonly Dictionary<Guid, PersistentEntity> entityTable = new();
        private readonly HashSet<Guid> entityHashset = new();
        private bool isLoading = false;

        private void Awake()
        {
            PersistentEntity.OnMarkedForDestroy += OnEntityRequestDestroy;

            foreach (PersistentEntity entity in entityList)
            {
                if (entity == null) throw new Exception();

                entityTable[entity.InstanceID] = entity;
            }

            ManagerCorePersistent.Instance.RegisterController(this);
        }
        private void OnDisable()
        {
            PersistentEntity.OnMarkedForDestroy -= OnEntityRequestDestroy;

            if (ManagerCorePersistent.HasInstance)
            {
                ManagerCorePersistent.Instance.UnregisterController(this);
            }         
        }

        private void OnEntityRequestDestroy(PersistentEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            if (!entity.IsMarkedForDestroy)
            {
                return;
            }

            if (!entityHashset.Contains(entity.InstanceID))
            {
                entityHashset.Add(entity.InstanceID);
            }

            TryUnregister(entity);
            Destroy(entity.gameObject);
        }
#if UNITY_EDITOR
        public void Populate()
        {
            entityList = GetComponentsInChildren<PersistentEntity>(true).ToList();
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

                    Debug.LogWarning($"Duplicate GUID fixed: {group.Key} on object {entity.name}, new GUID: {entity.InstanceID}");

                    UnityEditor.EditorUtility.SetDirty(entity);
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.LogWarning($"Collected {entityList.Count} objects!");
        }
#endif

        public bool IsRegistered(Guid id) => entityTable.ContainsKey(id);
        public bool TryRegister(GameObject gameObject, out PersistentEntity entity)
        {
            if (!gameObject.TryGetComponent(out entity))
            {
                Debug.LogError($"[{gameObject.name}] has no [PersistentInstanceEntity], Destroying [{gameObject.name}]");
                Destroy(gameObject);
                return false;
            }

            entity.GenerateID();
            return TryRegister(entity);
        }
        public bool TryRegister(PersistentEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (IsRegistered(entity.InstanceID))
            {
                Debug.LogError($"Trying to duplicate {entity.InstanceID}");
                return false;
            }

            entityList.Add(entity);
            entityTable[entity.InstanceID] = entity;
            return true;
        }
        public bool TryUnregister(PersistentEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (!IsRegistered(entity.InstanceID))
            {
                return false;
            }

            entityList.Remove(entity);
            entityTable.Remove(entity.InstanceID);
            return true;
        }

        public PersistentSceneData Export()
        {
            PersistentSceneData sceneData = new(ManagerCoreGame.Instance.GetCurrentScene(), new(), new());

            foreach (PersistentEntity entityObject in entityList)
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
            foreach (PersistentEntity entityObject in new List<PersistentEntity>(entityList))
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
                    if (gameObject.TryGetComponent(out PersistentEntity entityObject))
                    {
                        entityObject.Import(remainingData.Value);
                        TryRegister(entityObject);
                    }
                    else
                    {
                        Debug.LogWarning($"[PersistentInstanceEntity] component not found in [{gameObject.name}]");
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab not found for Orphan data: [{remainingData.Value.PrefabID}]");
                }
            }

            entityTable.Clear();

            foreach (PersistentEntity entity in entityList)
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