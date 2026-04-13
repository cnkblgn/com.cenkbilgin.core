using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCorePersistent : Manager<ManagerCorePersistent>
    {
        public bool IsLoading => sceneController != null && sceneController.IsLoading;

        private PersistentSceneController sceneController = null;

        private bool IsValid()
        {
            if (sceneController == null)
            {
                Debug.LogError("ManagerCorePersistent.IsValid() sceneController == null");
                return false;
            }

            return true;
        }

        public bool TryRegisterObject(GameObject gameObject, out PersistentInstanceEntity persistentObject)
        {
            persistentObject = null;

            if (!IsValid())
            {
                return false;
            }

            return sceneController.TryRegister(gameObject, out persistentObject);
        }
        public bool TryUnregisterObject(PersistentInstanceEntity persistentObject)
        {
            if (!IsValid())
            {
                return false;
            }

            Debug.LogWarning("ManagerCorePersistent.TryUnregisterObject() warning you are unregistering illegally! are you sure! this should handeld by 'PersistentSceneController'");

            return sceneController.TryUnregister(persistentObject);
        }
        public void RegisterController(PersistentSceneController sceneController)
        {
            if (!IsValid())
            {
                return;
            }

            this.sceneController = sceneController;
        }
        public void UnregisterController(PersistentSceneController sceneController)
        {
            if (this.sceneController != sceneController)
            {
                Debug.LogError("ManagerCorePersistent.UnregisterController() this.sceneController != sceneController");
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
