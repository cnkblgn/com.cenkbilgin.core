using System;
using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class UIViewportMesh : MonoBehaviour
    {
        public GameObject Connection => connection;
        public Vector3 Scale => scale; 
        public Vector3 Rotation => rotation; 
        public Vector3 Offset => offset;
        internal bool FollowTransform => followTransform;

        [Header("_")]
        [SerializeField, Required] private UIViewportItem prefab;
        [SerializeField, Required] private GameObject connection;
        [SerializeField, Required] private new MeshRenderer renderer;

        [Header("_")]
        [SerializeField] private bool followTransform = false;
        [SerializeField] private bool showOnStart = false;
        [SerializeField] private Vector3 scale = new(0.002f, 0.002f, 0.002f);
        [SerializeField] private Vector3 rotation = Vector3.zero;
        [SerializeField] private Vector3 offset = Vector3.zero;

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
            AddViewport();

            if (showOnStart)
            {
                ShowViewport(false);
            }
        }

        public void SyncViewport()
        {
            if (showOnStart)
            {
                ManagerUI.Instance.SyncViewport(instanceID, this);
            }
            else
            {
                ManagerUI.Instance.SyncViewport(baseID, this);
            }
        }
        public void AddViewport() => ManagerUI.Instance.AddViewport(prefab, showOnStart ? instanceID : null);
        public void ShowViewport(bool isInputFocused)
        {
            if (showOnStart)
            {
                ManagerUI.Instance.ShowViewport(instanceID, this, isInputFocused);
            }
            else
            {
                ManagerUI.Instance.ShowViewport(baseID, this, isInputFocused);
            }
        }
        public void HideViewport()
        {
            if (showOnStart)
            {
                ManagerUI.Instance.HideViewport(instanceID);
            }
            else
            {
                ManagerUI.Instance.HideViewport(baseID);
            }
        }
        public void FocusViewport(bool isFocused)
        {
            if (showOnStart)
            {
                ManagerUI.Instance.FocusViewport(instanceID, isFocused);
            }
            else
            {
                ManagerUI.Instance.FocusViewport(baseID, isFocused);
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
                Debug.LogWarning("UI Viewport mesh cull failed! renderer is null!?");
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
