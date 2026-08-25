using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class ViewportMesh : MonoBehaviour 
    {
        public GameObject Connection => connection;

        [Header("_")]
        [SerializeField, Required] private UIViewportView prefab = null;
        [SerializeField, Required] private GameObject connection = null;

        [Header("_")]
        [SerializeField] private bool debugVisibility = false;
        [SerializeField] private bool addOnAwake = true;
        [SerializeField] private bool showOnAwake = false;
        [SerializeField] private bool flipZ = false;

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

                if (showOnAwake)
                {
                    ShowViewport();
                }
            }
        }

        public void AddViewport() => ManagerUI.Instance.AddViewport(prefab);
        public void ShowViewport() => ManagerUI.Instance.ShowViewport(id, this);
        public void HideViewport() => ManagerUI.Instance.HideViewport(id);

        internal void ShowRenderer()
        {
            renderer.enabled = true;
            collider.enabled = true;
        }
        internal void HideRenderer()
        {
            renderer.enabled = false;
            collider.enabled = false;
        }

        internal bool CheckVisibility(Transform target, float minDistance, out float actualDistance)
        {
            actualDistance = float.MaxValue;

            if (renderer == null)
            {
                return false;
            }

            Vector3 vector = (renderer.bounds.center - target.position);
            actualDistance = vector.magnitude;

            if (actualDistance > minDistance)
            {
                return false;
            }

            if (actualDistance < 3.0f)
            {
                return true;
            }

            return IsFacingEachOther(transform.position, target.position, flipZ ? -transform.forward : transform.forward, target.forward, 0.1f, debugVisibility);
        }
    }
}