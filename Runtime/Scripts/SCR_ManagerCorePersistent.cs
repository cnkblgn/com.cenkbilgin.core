using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCorePersistent : Manager<ManagerCorePersistent>
    {
        public bool IsLoading => sceneController != null && sceneController.IsLoading;

        private PersistentSceneController sceneController = null;

        public bool TryRegisterObject(GameObject gameObject, out PersistentInstanceObject persistentObject)
        {
            persistentObject = null;

            if (sceneController == null)
            {
                LogError("ManagerCorePersistent.TrySpawnObject() sceneController == null");
                return false;
            }

            return sceneController.TryRegister(gameObject, out persistentObject);
        }

        public void RegisterController(PersistentSceneController sceneController)
        {
            if (sceneController == null)
            {
                LogError("ManagerCorePersistent.RegisterController() sceneController == null");
                return;
            }

            this.sceneController = sceneController;
        }
        public void UnregisterController(PersistentSceneController sceneController)
        {
            if (this.sceneController != sceneController)
            {
                LogError("ManagerCorePersistent.UnregisterController() this.sceneController != sceneController");
                return;
            }

            this.sceneController = null;
        }

        public void ExportTo(ref PersistentSceneData sceneData) => sceneController.ExportTo(ref sceneData);
        public void ImportFrom(ref PersistentSceneData sceneData) => sceneController.ImportFrom(ref sceneData);
    }
}
