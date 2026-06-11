using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class ManagerCorePersistent : Manager<ManagerCorePersistent>
    {
        public bool IsLoading => scene != null && scene.IsLoading;

        private PersistentScene scene = null;

        private bool IsValid()
        {
            if (scene == null)
            {
                Debug.LogError("sceneController == null");
                return false;
            }

            return true;
        }

        public bool IsRegistered(Guid id) => scene.IsRegistered(id);
        public bool TryRegisterEntity(GameObject gameObject, out PersistentEntity entity)
        {
            entity = null;

            if (!IsValid())
            {
                return false;
            }

            return scene.TryRegister(gameObject, out entity);
        }
        public bool TryRegisterEntity(PersistentEntity entity)
        {
            if (!IsValid())
            {
                return false;
            }

            return scene.TryRegister(entity);
        }
        public bool TryUnregisterEntity(PersistentEntity entity)
        {
            if (!IsValid())
            {
                return false;
            }

            Debug.LogWarning("Warning you are unregistering illegally! are you sure! this should handeld by 'PersistentSceneController'");

            return scene.TryUnregister(entity);
        }

        public void RegisterScene(PersistentScene scene)
        {
            if (!IsValid())
            {
                return;
            }

            this.scene = scene;
        }
        public void UnregisterScene(PersistentScene scene)
        {
            if (this.scene != scene)
            {
                Debug.LogError("this.scene != scene");
                return;
            }

            this.scene = null;
        }

        public PersistentSceneData Export()
        {
            if (!IsValid())
            {
                return null;
            }

            return scene.Export();
        }
        public void Import(PersistentSceneData sceneData)
        {
            if (!IsValid())
            {
                return;
            }

            scene.Import(sceneData);
        }
    }
}
