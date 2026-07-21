using System;
using System.Collections.Generic;

namespace Core.Persistence
{
    public static class PersistentSerializer
    {
        private const string KEY_ID = "id";
        private const string KEY_PREFAB = "prefab";
        private const string KEY_DESTROYED = "destroyed";
        private const string KEY_DATA = "data";
        private const string KEY_AVAILABLE_ENTITIES = "availableEntities";
        private const string KEY_DELETED_ENTITIES = "deletedEntities";

        public static void ExportEntityTo(this PersistentEntityData entity, Dictionary<string, DataNode> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.SetString(KEY_PREFAB, entity.PrefabID.Key);
            data.SetGuid(KEY_ID, entity.InstanceID);
            data.SetBool(KEY_DESTROYED, entity.IsMarkedForDestroy);
            data.SetData(KEY_DATA, entity.Data);
        }
        public static PersistentEntityData CreateEntityFrom(Dictionary<string, DataNode> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return new
            (
                new(data.GetString(KEY_PREFAB)),
                data.GetGuid(KEY_ID),
                data.GetBool(KEY_DESTROYED),
                data.GetData(KEY_DATA)
            );
        }

        public static void ExportSceneTo(this PersistentSceneData sceneData, Dictionary<string, DataNode> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            data.SetString(KEY_ID, sceneData.ID);

            Dictionary<string, DataNode> availableEntities = new();

            foreach (var availableEntity in sceneData.AvailableEntities)
            {
                PersistentEntityData entity = availableEntity.Value;

                Dictionary<string, DataNode> node = new();

                entity.ExportEntityTo(node);

                availableEntities.SetData(availableEntity.Key.ToString(), node);
            }

            data.SetData(KEY_AVAILABLE_ENTITIES, availableEntities);

            Dictionary<string, DataNode> deletedEntities = new();

            foreach (Guid deletedEntity in sceneData.DeletedEntities)
            {
                deletedEntities.SetGuid(deletedEntity.ToString(), deletedEntity);
            }

            data.SetData(KEY_DELETED_ENTITIES, deletedEntities);
        }
        public static PersistentSceneData CreateSceneFrom(Dictionary<string, DataNode> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            PersistentSceneData scene = new(data.GetString(KEY_ID));

            Dictionary<string, DataNode> availableEntities = data.GetData(KEY_AVAILABLE_ENTITIES);

            foreach (var availableEntity in availableEntities)
            {
                Dictionary<string, DataNode> node = availableEntities.GetData(availableEntity.Key);

                PersistentEntityData entity = CreateEntityFrom(node);

                scene.AvailableEntities[Guid.Parse(availableEntity.Key)] = entity;
            }

            Dictionary<string, DataNode> deletedEntities = data.GetData(KEY_DELETED_ENTITIES);

            foreach (var deletedEntity in deletedEntities)
            {
                scene.DeletedEntities.Add(Guid.Parse(deletedEntity.Key));
            }

            return scene;
        }
    }
}