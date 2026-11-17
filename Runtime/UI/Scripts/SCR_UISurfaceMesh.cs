using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class UISurfaceMesh : MonoBehaviour
    {
        public GameObject Connection => connection;
        public Vector3 Scale => scale; 
        public Vector3 Offset => offset; 

        [Header("_")]
        [SerializeField, Required] private UISurfaceItem prefab;
        [SerializeField, Required] private new MeshRenderer renderer;
        [SerializeField, Required] private GameObject connection;

        [Header("_")]
        [SerializeField] private Vector3 scale = new(0.002f, 0.002f, 0.002f);
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private bool showOnStart = false;

        [Header("_")]
        [SerializeField] private bool debugCulling = false;
        [SerializeField] private bool enableCulling = true;
        [SerializeField] private bool cullingFlipZ = false;
        [SerializeField, Min(0)] private float cullingDistance = 16;
        [SerializeField, Range(-1, 1)] private float cullingDotThreshold = 0.1f;

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

            if (prefab.HasInput)
            {
                ManagerUI.Instance.ShowCursor();
            }
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

            if (prefab.HasInput)
            {
                ManagerUI.Instance.HideCursor();
            }
        }
        public void FocusSurface(bool isFocused)
        {
            if (showOnStart)
            {
                ManagerUI.Instance.FocusSurface(instanceID, isFocused);
            }
            else
            {
                ManagerUI.Instance.FocusSurface(baseID, isFocused);
            }
        }

        internal bool TryCull(Transform target, out float distance, out float dot)
        {
            distance = float.MaxValue;
            dot = -1f;

            if (!enableCulling)
            {
                return false;
            }

            if (renderer == null)
            {
                Debug.LogWarning("UI Surface mesh cull failed! renderer is null!?");
                return false;
            }

            Vector3 center = renderer.bounds.center;

            distance = Vector3.Distance(center, target.position);

            if (distance > cullingDistance)
            {
                return true;
            }

            bool facing = IsFacingEachOther(target.position, center, target.forward, cullingFlipZ ? -transform.forward : transform.forward, cullingDotThreshold, out float aDot, out float bDot, debugCulling);

            dot = Mathf.Min(aDot, bDot);

            return !facing;
        }
    }
}
