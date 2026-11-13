using System;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class UISurfaceItem : MonoBehaviour
    {
        internal string ID => id;
        internal Guid InstanceID => instanceID;
        internal bool IsActive => isActive;
        internal bool IsRendering => isRendering;
        internal bool HasInput => receiveInput;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField] private bool receiveInput = false;

        private Canvas thisCanvas;
        private Camera thisCamera;
        private UISurfaceMesh thisMesh;
        private Guid instanceID;
        private bool isInitialized = false;
        private bool isRendering = false;
        private bool isActive = false;

        private void OnDisable() => OnDeinitialized();

        /// <summary> Called every frame. </summary>
        protected abstract void OnTick(float deltaTime);
        /// <summary> Called when created. </summary>
        protected abstract void OnInitialized();
        /// <summary> Called when destroyed. </summary>
        protected abstract void OnDeinitialized();
        /// <summary> Called when show. </summary>
        protected abstract void OnShow(UISurfaceMesh surface);
        /// <summary> Called when hide. </summary>
        protected abstract void OnHide();

        internal void Tick(Camera camera, float deltaTime)
        {
            if (!isActive)
            {
                return;
            }

            thisCamera = camera;

            if (receiveInput && thisCanvas.worldCamera == null)
            {
                thisCanvas.worldCamera = thisCamera;
            }

            OnTick(deltaTime);
        }

        internal bool TryCull(Transform target)
        {
            if (!isActive)
            {
                return false;
            }

            bool isCulled = thisMesh.TryCull(target, out _, out _);

            if (isCulled)
            {
                HideCanvas();
            }
            else
            {
                ShowCanvas();
            }

            return isCulled;
        }

        internal void Initialize(Camera camera, Guid instanceID)
        {
            if (isInitialized)
            {
                return;
            }

            this.instanceID = instanceID;
            thisCamera = camera;
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            isInitialized = true;
            isActive = false;

            OnInitialized();
        }
        internal void Deinitialize()
        {
            if (!isInitialized)
            {
                return;
            }

            isInitialized = false;
            OnDeinitialized();
        }

        internal void ShowSurface(UISurfaceMesh mesh)
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            transform.SetPositionAndRotation(mesh.transform.position, Quaternion.LookRotation(mesh.transform.forward));
            transform.localScale = Vector3.one * mesh.Scale;

            OnShow(thisMesh = mesh);
            ShowCanvas();
        }
        internal void HideSurface()
        {
            if (!isActive)
            {
                return;
            }

            OnHide();
            HideCanvas();
   
            isActive = false;
        }

        private void ShowCanvas()
        {
            if (!isActive)
            {
                return;
            }

            if (isRendering)
            {
                return;
            }

            isRendering = true;
            thisCanvas.Show();

            if (receiveInput)
            {
                thisCanvas.worldCamera = thisCamera;
            }
        }
        private void HideCanvas()
        {
            if (!isActive)
            {
                return;
            }

            if (!isRendering)
            {
                return;
            }

            isRendering = false;
            thisCanvas.Hide();
        }
    }
}