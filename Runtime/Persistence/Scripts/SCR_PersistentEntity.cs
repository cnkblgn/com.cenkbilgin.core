using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Prefab;

namespace Core.Persistence
{
    [DisallowMultipleComponent]
    public abstract class PersistentEntity : MonoBehaviour
    {
        public static event Action<PersistentEntity> OnMarkedForDestroy;

        public abstract PrefabID PrefabID { get; }
        public Guid InstanceID => instanceID;
        public bool IsMarkedForDestroy => isMarkedForDestroy;

        [Header("_")]
        [SerializeField] private string _instanceID = "";

        private Guid instanceID = default;
        private bool isQuitting = false;
        private bool isMarkedForDestroy = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => OnMarkedForDestroy = null;

        protected virtual void Start()
        {
            if (instanceID == Guid.Empty)
            {
                Debug.LogError($"InstanceID missing for {gameObject.name}", gameObject);
            }
        }
        private void OnApplicationQuit() => isQuitting = true;
        private void OnDestroy()
        {
            if (isQuitting)
            {
                return;
            }

            if (!IsMarkedForDestroy && Application.isPlaying && !PersistentDatabase.IsLoading && !ManagerGame.Instance.IsLoading)
            {
                Debug.LogError($"[{name}] destroyed without persistence or destroyed illegally");
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

        public void MarkForDestroy()
        {
            if (isMarkedForDestroy)
            {
                return;
            }

            isMarkedForDestroy = true;
            OnMarkedForDestroy?.Invoke(this);
        }

        public PersistentEntityData Export()
        {
            Dictionary<string, DataNode> data = new();

            OnExported(data);

            return new(PrefabID, instanceID, isMarkedForDestroy, data);
        }
        protected abstract void OnExported(Dictionary<string, DataNode> data);
        public void Import(PersistentEntityData data)
        {
            instanceID = data.InstanceID;
            isMarkedForDestroy = data.IsMarkedForDestroy;

#if UNITY_EDITOR
            _instanceID = instanceID.ToString();
#endif
            OnImported(data.Data);
        }
        protected abstract void OnImported(Dictionary<string, DataNode> data);
    }
}
