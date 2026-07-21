using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Persistence
{
    public static class PersistentDatabase
    {
        public static bool IsLoading => isLoading;

        private static readonly Dictionary<Guid, PersistentEntity> availableEntities = new();
        private static readonly HashSet<Guid> deletedEntities = new();
        private static bool isLoading = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            PersistentEntity.OnMarkedForDestroy += OnEntityRequestDestroy;

            availableEntities.Clear();
            deletedEntities.Clear();
        }
        private static void OnEntityRequestDestroy(PersistentEntity entity)
        {
            if (isLoading)
            {
                return;
            }

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
            GameObject.Destroy(entity.gameObject);
        }

        public static bool IsRegistered(Guid id) => availableEntities.ContainsKey(id);
        public static bool TryRegister(GameObject gameObject, out PersistentEntity entity)
        {
            if (!gameObject.TryGetComponent(out entity))
            {
                Debug.LogError($"Trying to register [{gameObject.name}] but has no [PersistentEntity] component", gameObject);

                return false;
            }

            entity.GenerateID();
            return TryRegister(entity);
        }
        public static bool TryRegister(PersistentEntity entity)
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

            availableEntities[entity.InstanceID] = entity;
            return true;
        }
        public static bool TryUnregister(PersistentEntity entity)
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

            availableEntities.Remove(entity.InstanceID);
            return true;
        }

        public static PersistentSceneData Export()
        {
            PersistentSceneData sceneData = new(ManagerGame.Instance.GetCurrentScene());

            foreach (PersistentEntity entity in availableEntities.Values)
            {
                if (entity == null || entity.IsMarkedForDestroy)
                {
                    continue;
                }

                sceneData.AvailableEntities[entity.InstanceID] = entity.Export();
            }

            foreach (Guid id in deletedEntities)
            {
                sceneData.DeletedEntities.Add(id);
            }

            return sceneData;
        }
        public static void Import(PersistentSceneData sceneData)
        {
            isLoading = true;

            deletedEntities.Clear();
            deletedEntities.UnionWith(sceneData.DeletedEntities);

            foreach (PersistentEntity entity in new List<PersistentEntity>(availableEntities.Values))
            {
                Guid id = entity.InstanceID;

                if (deletedEntities.Contains(id))
                {
                    entity.MarkForDestroy();

                    TryUnregister(entity);

                    GameObject.Destroy(entity.gameObject);
                    continue;
                }

                if (sceneData.AvailableEntities.TryGetValue(id, out var incomingData))
                {
                    entity.Import(incomingData);
                }
                else
                {
                    // Save'de yoksa → yeni versiyonda eklenmiş obje
                }
            }

            // Save’de var ama sahnede yok → dinamik spawn
            foreach (var remainingEntities in sceneData.AvailableEntities)
            {
                if (availableEntities.ContainsKey(remainingEntities.Key))
                {
                    continue;
                }

                if (remainingEntities.Value.PrefabID.TrySpawn(Vector3.zero, Quaternion.identity, null, out GameObject gameObject))
                {
                    if (gameObject.TryGetComponent(out PersistentEntity entity))
                    {
                        entity.Import(remainingEntities.Value);
                        TryRegister(entity);
                    }
                    else
                    {
                        Debug.LogWarning($"[PersistentInstanceEntity] component not found in [{gameObject.name}]");
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab not found for Orphan data: [{remainingEntities.Value.PrefabID}]");
                }
            }

            isLoading = false;
        }
    }
}
