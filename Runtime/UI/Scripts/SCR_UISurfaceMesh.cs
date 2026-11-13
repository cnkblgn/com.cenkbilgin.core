using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class UISurfaceMesh : MonoBehaviour
    {
        public GameObject Connection => connection;
        public float Scale => scale; 

        [Header("_")]
        [SerializeField, Required] private UISurfaceItem prefab;
        [SerializeField, Required] private GameObject connection;

        [Header("_")]
        [SerializeField, Range(0, 1)] private float scale = 0.0025;
        [SerializeField] private bool showOnStart = false;

        [Header("_")]
        [SerializeField] private bool debugCulling = false;
        [SerializeField] private bool enableCulling = true;
        [SerializeField] private bool cullingFlipZ = false;
        [SerializeField, Min(0)] private float cullingDistance = 16;
        [SerializeField, Range(-1, 1)] private float cullingDotThreshold = 0.1f;

        private MeshRenderer meshRenderer;
        private string baseID = STRING_NULL;
        private Guid instanceID;

        private void Awake()
        {
            if (prefab == null)
            {
                throw new NullReferenceException($"UI Overlay prefab not found! {nameof(prefab)}");
            }

            if (connection == null)
            {
                throw new NullReferenceException($"UI Overlay connection not found! {nameof(connection)}");
            }

            baseID = prefab.ID;
            instanceID = Guid.NewGuid();
            meshRenderer = GetComponent<MeshRenderer>();
        }
        private void Start()
        {
            AddSurface();

            if (showOnStart)
            {
                ShowSurface();
            }
        }

        public void AddSurface() => ManagerUI.Instance.AddSurface(prefab, showOnStart ? instanceID : null);
        public void ShowSurface()
        {
            if (showOnStart)
            {
                ManagerUI.Instance.ShowSurface(instanceID, this);
            }
            else
            {
                ManagerUI.Instance.ShowSurface(baseID, this);
            }
            
            meshRenderer.enabled = false;
        }
        public void HideSurface()
        {
            if (showOnStart)
            {
                ManagerUI.Instance.HideSurface(instanceID);
            }
            else
            {
                ManagerUI.Instance.HideSurface(baseID);
            }

            meshRenderer.enabled = true;
        }

        internal bool TryCull(Transform target, out float distance, out float dot)
        {
            distance = float.MaxValue;
            dot = -1f;

            if (enableCulling)
            {
                return false;
            }

            Vector3 center = meshRenderer.bounds.center;

            distance = Vector3.Distance(center, target.position);

            if (distance > cullingDistance)
            {
                return false;
            }

            bool facing = IsFacingEachOther(target.position, center, target.forward, cullingFlipZ ? -transform.forward : transform.forward, cullingDotThreshold, out float aDot, out float bDot, debugCulling);

            dot = Mathf.Min(aDot, bDot);

            return facing;
        }
    }
}
