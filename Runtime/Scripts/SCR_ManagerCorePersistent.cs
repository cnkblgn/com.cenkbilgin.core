using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCorePersistent : Manager<ManagerCorePersistent>
    {
        public bool IsLoading => sceneController != null && sceneController.IsLoading;

        private PersistentSceneController sceneController = null;

        public bool TryRegisterObject(GameObject gameObject, out PersistentInstanceEntity persistentObject)
        {
            persistentObject = null;

            if (sceneController == null)
            {
                Debug.LogError("ManagerCorePersistent.TrySpawnObject() sceneController == null");
                return false;
            }

            return sceneController.TryRegister(gameObject, out persistentObject);
        }
        public void RegisterController(PersistentSceneController sceneController)
        {
            if (sceneController == null)
            {
                Debug.LogError("ManagerCorePersistent.RegisterController() sceneController == null");
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
            if (sceneController == null)
            {
                Debug.LogError("ManagerCorePersistent.Export() sceneController == null");
            }

            return sceneController.Export();
        }
        public void Import(PersistentSceneData sceneData)
        {
            if (sceneController == null)
            {
                Debug.LogError("ManagerCorePersistent.Import() sceneController == null");
            }

            sceneController.Import(sceneData);
        }
    }
}
