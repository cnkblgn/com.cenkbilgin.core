using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class PersistentScene : MonoBehaviour
    {
        public bool IsLoading => isLoading;

        [Header("_")]
        [SerializeField, Required] private List<PersistentEntity> entityList = new();

        private readonly Dictionary<Guid, PersistentEntity> entityTable = new();
        private readonly HashSet<Guid> deletedEntities = new();
        private bool isLoading = false;

        private void Awake()
        {
            PersistentEntity.OnMarkedForDestroy += OnEntityRequestDestroy;

            foreach (PersistentEntity entity in entityList)
            {
                if (entity == null) throw new Exception();

                entityTable[entity.InstanceID] = entity;
            }

            ManagerCorePersistent.Instance.RegisterScene(this);
        }
        private void OnDisable()
        {
            PersistentEntity.OnMarkedForDestroy -= OnEntityRequestDestroy;

            if (ManagerCorePersistent.HasInstance)
            {
                ManagerCorePersistent.Instance.UnregisterScene(this);
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

            if (!deletedEntities.Contains(entity.InstanceID))
            {
                deletedEntities.Add(entity.InstanceID);
            }

            TryUnregister(entity);
            Destroy(entity.gameObject);
        }

#if UNITY_EDITOR
        [Clickable("Populate")]
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
                Debug.LogError($"Trying to register [{gameObject.name}] but has no [PersistentEntity] component", gameObject);

                return false;
            }

            entity.GenerateID();
            return TryRegister(entity);
        }
        public bool TryRegister(PersistentEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError($"Trying to register null persistent entity!");
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
                Debug.LogError($"Trying to unregister null persistent entity!");
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
            PersistentSceneData sceneData = new(ManagerCoreGame.Instance.GetCurrentScene());

            foreach (PersistentEntity entityObject in entityList)
            {
                if (entityObject == null || entityObject.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.AvailableEntities[entityObject.InstanceID] = entityObject.Export();
            }

            foreach (Guid id in deletedEntities)
            {
                sceneData.DeletedEntities.Add(id);
            }

            return sceneData;
        }
        public void Import(PersistentSceneData sceneData)
        {
            isLoading = true;

            deletedEntities.Clear();
            deletedEntities.UnionWith(sceneData.DeletedEntities);

            foreach (PersistentEntity entityObject in new List<PersistentEntity>(entityList))
            {
                Guid id = entityObject.InstanceID;

                if (deletedEntities.Contains(id))
                {
                    entityObject.MarkForDestroy(true);
                    TryUnregister(entityObject);
                    Destroy(entityObject.gameObject);
                    continue;
                }

                if (sceneData.AvailableEntities.TryGetValue(id, out var incomingData))
                {
                    entityObject.Import(incomingData);
                }
                else { } // Save'de yoksa → yeni versiyonda eklenmiş obje
            }

            // Save’de var ama sahnede yok → dinamik spawn
            foreach (var remainingData in sceneData.AvailableEntities)
            {
                if (entityTable.ContainsKey(remainingData.Key))
                {
                    continue;
                }

                if (ManagerCorePrefab.Instance.TrySpawn(new PrefabID(remainingData.Value.PrefabID), Vector3.zero, Quaternion.identity, null, out GameObject gameObject))
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