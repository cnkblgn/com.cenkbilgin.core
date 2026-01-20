using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public abstract class PersistentInstanceEntity : MonoBehaviour
    {
        public static event Action<PersistentInstanceEntity> OnMarkedForDestroy;

        public abstract string TypeID { get; }
        public abstract string PrefabID { get; }
        public Guid InstanceID => instanceID;
        public bool IsMarkedForDestroy => isMarkedForDestroy;

        [Header("_")]
        [SerializeField] private string _instanceID = "";

        private Guid instanceID = default;
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
                Debug.LogError($"PersistentInstanceEntity.OnDestroy() [{name}] destroyed without persistence or destroyed illegally");
            }
        }
        protected virtual void Start()
        {
            if (instanceID == Guid.Empty)
            {
                Debug.LogError($"PersistentInstanceEntity.Start() instanceID missing for {gameObject.name}", gameObject);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(_instanceID))
            {
                instanceID = Guid.Parse(_instanceID);
            }
        }
#endif
        internal void GenerateID(bool force = false)
        {
            if (instanceID != Guid.Empty && !force)
            {
                return;
            }

            if (string.IsNullOrEmpty(_instanceID) || force)
            {
                instanceID = Guid.NewGuid();
            }
            else
            {
                instanceID = Guid.Parse(_instanceID);
            }

            _instanceID = instanceID.ToString();
        }
        public void MarkForDestroy(bool isSilent = false)
        {
            if (isMarkedForDestroy)
            {
                return;
            }

            isMarkedForDestroy = true;
            if (!isSilent) OnMarkedForDestroy?.Invoke(this);
        }
        public PersistentInstanceData Export() => new(TypeID, PrefabID, instanceID, isMarkedForDestroy, ExportInternal());
        protected abstract Dictionary<string, PersistentValue> ExportInternal();
        public void Import(PersistentInstanceData data)
        {
            instanceID = data.InstanceID;
            isMarkedForDestroy = data.IsMarkedForDestroy;

#if UNITY_EDITOR
            _instanceID = instanceID.ToString();
#endif
            ImportInternal(data.Data);
        }
        protected abstract void ImportInternal(Dictionary<string, PersistentValue> data);
    }
}
