using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public abstract class UIViewportItem : MonoBehaviour
    {
        internal string ID => id;
        internal Guid InstanceID => instanceID;
        internal bool IsActive => isActive;
        internal bool IsRendering => isRendering;
        internal bool HasInput => receiveInput;

        [Header("_")]
        [SerializeField, Required] private string id = string.Empty;
        [SerializeField] private bool receiveInput = false;

        protected Canvas thisCanvas;
        protected GraphicRaycaster[] thisRaycasters;
        private Camera thisCamera;
        private UIViewportMesh thisMesh;
        private Guid instanceID;
        private bool isInitialized = false;
        private bool isRendering = false;
        private bool isActive = false;

        /// <summary> Called every frame. </summary>
        protected abstract void OnTick(float deltaTime);
        /// <summary> Called when created. </summary>
        protected abstract void OnInitialized();
        /// <summary> Called when destroyed. </summary>
        protected abstract void OnDeinitialized();
        /// <summary> Called when show. </summary>
        protected abstract void OnShow(UIViewportMesh surface);
        /// <summary> Called when hide. </summary>
        protected abstract void OnHide();
        /// <summary> Called when focused. </summary>
        protected abstract void OnFocus(bool isFocused);

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
            thisRaycasters = GetComponentsInChildren<GraphicRaycaster>();
            thisCanvas = GetComponent<Canvas>();
            thisCanvas.Hide();

            for (int i = 0; i < thisRaycasters.Length; i++)
            {
                thisRaycasters[i].enabled = false;
            }

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

        internal void ShowViewport(UIViewportMesh mesh, bool isInputFocused)
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            transform.SetParent(mesh.transform);
            transform.SetPositionAndRotation(mesh.transform.position, Quaternion.identity);
            transform.localScale = mesh.Scale;
            transform.localRotation = Quaternion.Euler(mesh.Rotation);
            transform.position += transform.rotation * mesh.Offset;

            OnShow(thisMesh = mesh);

            FocusViewport(true, isInputFocused);

            ShowCanvas();
        }
        internal void HideViewport()
        {
            if (!isActive)
            {
                return;
            }

            FocusViewport(false, true);

            OnHide();

            HideCanvas();
   
            isActive = false;
        }
        internal void FocusViewport(bool isFocused, bool isInputFocused)
        {
            if (!isActive)
            {
                return;
            }

            if (receiveInput && isInputFocused)
            {
                if (isFocused)
                {
                    ManagerUI.Instance.ShowCursor();

                    for (int i = 0; i < thisRaycasters.Length; i++)
                    {
                        thisRaycasters[i].enabled = true;
                    }
                }
                else
                {
                    ManagerUI.Instance.HideCursor();

                    for (int i = 0; i < thisRaycasters.Length; i++)
                    {
                        thisRaycasters[i].enabled = false;
                    }
                }
                
            }

            OnFocus(isFocused);
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