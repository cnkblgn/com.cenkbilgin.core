using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [DefaultExecutionOrder(100)]
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

            if (!isActive || !thisMesh.FollowTransform)
            {
                return;
            }

            ApplyMeshTransform();
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

        private void ApplyMeshTransform()
        {
            Transform meshTransform = thisMesh.transform;
            Quaternion targetRotation = meshTransform.rotation * Quaternion.Euler(thisMesh.Rotation);
            Vector3 targetPosition = meshTransform.position + targetRotation * thisMesh.Offset;
            Vector3 targetScale = Vector3.Scale(meshTransform.lossyScale, thisMesh.Scale);

            transform.SetPositionAndRotation(targetPosition, targetRotation);

            Transform parent = transform.parent;

            if (parent == null)
            {
                transform.localScale = targetScale;
            }
            else
            {
                Vector3 parentScale = parent.lossyScale;

                transform.localScale = new Vector3(
                    parentScale.x != 0f ? targetScale.x / parentScale.x : 0f,
                    parentScale.y != 0f ? targetScale.y / parentScale.y : 0f,
                    parentScale.z != 0f ? targetScale.z / parentScale.z : 0f
                );
            }
        }
        internal void ShowViewport(UIViewportMesh mesh, bool isInputFocused)
        {
            if (isActive)
            {
                return;
            }

            isActive = true;
            thisMesh = mesh;

            if (mesh.FollowTransform)
            {
                ApplyMeshTransform();
            }
            else
            {
                transform.SetParent(mesh.transform);
                transform.SetPositionAndRotation(mesh.transform.position, Quaternion.identity);
                transform.localScale = mesh.Scale;
                transform.localRotation = Quaternion.Euler(mesh.Rotation);
                transform.position += transform.rotation * mesh.Offset;
            }

            OnShow(mesh);

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