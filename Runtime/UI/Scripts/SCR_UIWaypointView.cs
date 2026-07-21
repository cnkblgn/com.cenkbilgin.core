using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class UIWaypointView : MonoBehaviour
    {
        public UIWaypointData Data { get; private set; }
        public bool IsCompleted { get; private set; }

        [Header("_")]
        [SerializeField, Required] private Image icon = null;
        [SerializeField, Required] private TextMeshProUGUI text = null;

        [Header("_")]
        [SerializeField] private bool showDistance = false;
        [SerializeField] private bool hideBehind = false;

        private CanvasGroup thisCanvas = null;
        private RectTransform thisTransform = null;
        private Action completeCallback = null;
        private Vector3 offset = Vector3.zero;
        private float textTimer = 0;
        private float cachedWidth = 0;
        private float cachedHeight = 0;
        private bool isInitialized = false;
        private bool isVisible = false;
        private bool isActive = false;

        public void Tick(Camera cameraController, Transform cameraTransform)
        {
            if (!isActive)
            {
                return;
            }

            if (cameraController == null || cameraTransform == null)
            {
                Complete();
                return;
            }

            if (Data.HasTarget && Data.TargetTransform == null)
            {
                Complete();
                return;
            }

            if (showDistance)
            {
                textTimer += Time.deltaTime;

                if (textTimer >= 0.5f)
                {
                    float distance = Vector3.Distance(Data.Position, cameraTransform.position);
                    string distanceString = $"{(int)distance} m";

                    text.text = string.IsNullOrEmpty(Data.Text) ? distanceString : $"{Data.Text}\n{distanceString}";
                    textTimer = 0;
                }
            }

            float minX = cachedWidth;
            float maxX = Screen.width - minX;
            float minY = cachedHeight;
            float maxY = Screen.height - minY;

            Vector3 worldPosition = Data.Position + offset;
            Vector3 screenPosition = cameraController.WorldToScreenPoint(worldPosition);
            bool isBehind = screenPosition.z < 0f;

            if (isBehind)
            {
                screenPosition.x = Screen.width - screenPosition.x;
                screenPosition.y = Screen.height - screenPosition.y;
            }

            if (hideBehind)
            {
                bool isOffscreen = isBehind || screenPosition.x < 0f || screenPosition.x > Screen.width || screenPosition.y < 0f || screenPosition.y > Screen.height;

                if (isOffscreen)
                {
                    if (isVisible)
                    {
                        thisCanvas.Hide();
                        isVisible = false;
                    }
                }
                else
                {
                    if (!isVisible)
                    {
                        thisCanvas.Show(false, false);
                        isVisible = true;
                    }
                }
            }

            screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
            screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);
            thisTransform.position = screenPosition;
        }

        internal void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            thisTransform = GetComponent<RectTransform>();
            thisCanvas = GetComponent<CanvasGroup>();
            completeCallback = Complete;

            isInitialized = true;
        }
        internal void Deinitialize()
        {
            if (!isInitialized)
            {
                return;
            }

            Complete();

            isActive = false;
            IsCompleted = false;
        }

        public void Show(in UIWaypointData data, Vector3 offset)
        {
            if (!isInitialized)
            {
                return;
            }

            gameObject.SetActive(true);

            Data = data;
            isActive = true;
            IsCompleted = false;
            isVisible = true;

            this.offset = offset;
            text.text = Data.Text;
            icon.color = Data.Color;
            icon.sprite = Data.Icon != null ? Data.Icon : icon.sprite;

            cachedWidth = thisTransform.rect.width * 0.5f;
            cachedHeight = thisTransform.rect.height * 0.5f;

            thisTransform.localScale = Vector3.zero;
            thisTransform.Scale(Vector3.one, 0.25f);
        }
        public void Hide()
        {
            if (!isInitialized || !isActive)
            {
                return;
            }

            isActive = false;
            thisTransform.Scale(Vector3.zero, 0.25f, 0.25f, TweenType.SCALED, EaseType.LINEAR, completeCallback);
        }

        private void Complete()
        {
            IsCompleted = true;
            thisCanvas.Show(false, false);
            gameObject.SetActive(false);
        }
    }
}