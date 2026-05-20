using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class ManagerCorePersistent : Manager<ManagerCorePersistent>
    {
        public bool IsLoading => sceneController != null && sceneController.IsLoading;

        private PersistentSceneController sceneController = null;

        private bool IsValid()
        {
            if (sceneController == null)
            {
                Debug.LogError("sceneController == null");
                return false;
            }

            return true;
        }

        public bool IsRegistered(Guid id) => sceneController.IsRegistered(id);
        public bool TryRegisterEntity(GameObject gameObject, out PersistentInstanceEntity entity)
        {
            entity = null;

            if (!IsValid())
            {
                return false;
            }

            return sceneController.TryRegister(gameObject, out entity);
        }
        public bool TryRegisterEntity(PersistentInstanceEntity entity)
        {
            if (!IsValid())
            {
                return false;
            }

            return sceneController.TryRegister(entity);
        }
        public bool TryUnregisterEntity(PersistentInstanceEntity entity)
        {
            if (!IsValid())
            {
                return false;
            }

            Debug.LogWarning("Warning you are unregistering illegally! are you sure! this should handeld by 'PersistentSceneController'");

            return sceneController.TryUnregister(entity);
        }

        public void RegisterController(PersistentSceneController controller)
        {
            if (!IsValid())
            {
                return;
            }

            this.sceneController = controller;
        }
        public void UnregisterController(PersistentSceneController controller)
        {
            if (this.sceneController != controller)
            {
                Debug.LogError("this.sceneController != sceneController");
                return;
            }

            this.sceneController = null;
        }

        public PersistentSceneData Export()
        {
            if (!IsValid())
            {
                return null;
            }

            return sceneController.Export();
        }
        public void Import(PersistentSceneData sceneData)
        {
            if (!IsValid())
            {
                return;
            }

            sceneController.Import(sceneData);
        }
    }
}
