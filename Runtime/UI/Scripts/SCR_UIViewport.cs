using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class UIViewport : MonoBehaviour 
    {
        public GameObject Connection => connection;

        [Header("_")]
        [SerializeField, Required] private UIViewportItem prefab = null;
        [SerializeField, Required] private GameObject connection = null;

        [Header("_")]
        [SerializeField] private bool addOnAwake = true;

        private new MeshRenderer renderer = null;
        private new MeshCollider collider = null;
        private string id = STRING_NULL;

        private void Awake()
        {
            if (prefab == null)
            {
                throw new NullReferenceException($"Viewport prefab not found! {nameof(prefab)}");
            }

            if (connection == null)
            {
                throw new NullReferenceException($"Viewport connection not found! {nameof(prefab)}");
            }

            renderer = GetComponent<MeshRenderer>();
            collider = GetComponent<MeshCollider>();
            id = prefab.ID;

            HideRenderer();
        }
        private void Start()
        {
            if (addOnAwake)
            {
                AddViewport();
            }
        }

        public void AddViewport() => ManagerUI.Instance.AddViewport(prefab);
        public void ShowViewport() => ManagerUI.Instance.ShowViewport(id, this);
        public void HideViewport() => ManagerUI.Instance.HideViewport(id);

        internal void ShowRenderer() => renderer.enabled = true;
        internal void HideRenderer() => renderer.enabled = false;
    }
}