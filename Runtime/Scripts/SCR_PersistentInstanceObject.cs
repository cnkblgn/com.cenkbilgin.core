using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public abstract class PersistentInstanceObject : MonoBehaviour
    {
        public static event Action<PersistentInstanceObject> OnMarkedForDestroy;

        public abstract string TypeID { get; }
        public string InstanceID => instanceID;
        public string PrefabID => prefabID;
        public bool IsMarkedForDestroy => isMarkedForDestroy;

        [Header("_")]
        [SerializeField] private string prefabID = null;
        [SerializeField, ReadOnly] private string instanceID = null;

        private bool isQuitting = false;
        private bool isMarkedForDestroy = false;

        private void OnApplicationQuit() => isQuitting = true;
        private void OnDestroy()
        {
            if (isQuitting)
            {
                return;
            }

            if (!IsMarkedForDestroy && Application.isPlaying && !ManagerCorePersistent.Instance.IsLoading && !ManagerCoreGame.Instance.IsLoading)
            {
                LogError($"PersistentInstanceObject.OnDestroy() [{name}] destroyed without persistence or destroyed illegally");
            }
        }
        protected virtual void Start()
        {
            if (string.IsNullOrEmpty(InstanceID))
            {
                LogError($"PersistentInstanceObject.Start() InstanceID missing for {gameObject.name}", gameObject);
            }
        }

        internal void SetID(string value)
        {
            if (!string.IsNullOrEmpty(instanceID))
            {
                LogError($"PersistentInstanceObject.SetID() InstanceID already set for {gameObject.name}");
                return;
            }

            instanceID = value;
        }
        public void MarkForDestroy()
        {
            if (isMarkedForDestroy)
            {
                return;
            }

            isMarkedForDestroy = true;
            OnMarkedForDestroy?.Invoke(this);
        }

        public PersistentInstanceData Export() => new(TypeID, prefabID, instanceID, isMarkedForDestroy, ExportInternal());
        protected abstract Dictionary<string, PersistentValue> ExportInternal();
        public void Import(PersistentInstanceData data)
        {
            prefabID = data.PrefabID;
            instanceID = data.InstanceID;
            isMarkedForDestroy = data.IsMarkedForDestroy;
            ImportInternal(data.Data);
        }
        protected abstract void ImportInternal(Dictionary<string, PersistentValue> data);
    }
}
