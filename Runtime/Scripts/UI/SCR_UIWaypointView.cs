using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    internal sealed class UIWaypointView : MonoBehaviour
    {
        public UIWaypointData Data { get; private set; }
        public bool IsCompleted { get; private set; }

        [Header("_")]
        [SerializeField, Required] private Image iconImage = null;
        [SerializeField, Required] private TextMeshProUGUI nameText = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI distanceText = null;

        private RectTransform thisTransform = null;
        private Action completeCallback = null;
        private Vector3 offset = Vector3.zero;
        private float textTimer = 0;
        private float cachedWidth = 0;
        private float cachedHeight = 0;
        private bool isInitialized = false;
        private bool isActive = false;

        public void Tick(Camera cameraController, Transform cameraTransform, Rect rectBounds)
        {
            if (!TryTickInternal(cameraController, cameraTransform))
            {
                return;
            }

            ClampToRect(cameraController, rectBounds);
        }
        public void Tick(Camera cameraController, Transform cameraTransform)
        {
            if (!TryTickInternal(cameraController, cameraTransform))
            {
                return;
            }

            ClampToScreen(cameraController);
        }
        private bool TryTickInternal(Camera cameraController, Transform cameraTransform)
        {
            if (!isActive)
            {
                return false;
            }

            if (cameraController == null || cameraTransform == null)
            {
                Complete();
                return false;
            }

            if (Data.HasTarget && Data.TargetTransform == null)
            {
                Complete();
                return false;
            }

            if (distanceText != null)
            {
                textTimer += Time.deltaTime;

                if (textTimer >= 0.5f)
                {
                    float distance = Vector3.Distance(Data.Position, cameraTransform.position);
                    distanceText.text = $"{(int)distance} m";
                    textTimer = 0;
                }
            }

            return true;
        }

        private void ClampToScreen(Camera cameraController)
        {
            float minX = cachedWidth;
            float maxX = Screen.width - minX;
            float minY = cachedHeight;
            float maxY = Screen.height - minY;

            Vector3 worldPosition = Data.Position + offset;
            Vector3 screenPosition = cameraController.WorldToScreenPoint(worldPosition);

            if (screenPosition.z < 0f)
            {
                screenPosition.x = Screen.width - screenPosition.x;
                screenPosition.y = Screen.height - screenPosition.y;
            }

            screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
            screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);
            thisTransform.position = screenPosition;
        }
        private void ClampToRect(Camera cameraController, Rect rectBounds)
        {
            Vector3 worldPosition = Data.Position + offset;
            Vector3 viewport = cameraController.WorldToViewportPoint(worldPosition);

            if (viewport.z < 0f)
            {
                viewport.x = -viewport.x;
            }

            float minX = rectBounds.xMin + cachedWidth;
            float maxX = rectBounds.xMax - cachedWidth;
            float minY = rectBounds.yMin + cachedHeight;
            float maxY = rectBounds.yMax - cachedHeight;

            Vector2 localPosition = new((viewport.x - 0.5f) * rectBounds.width, (viewport.y - 0.5f) * rectBounds.height);

            localPosition.x = Mathf.Clamp(localPosition.x, minX, maxX);
            localPosition.y = Mathf.Clamp(localPosition.y, minY, maxY);

            thisTransform.anchoredPosition = localPosition;
        }

        internal void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            thisTransform = GetComponent<RectTransform>();
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

            this.offset = offset;
            nameText.text = Data.Text;
            iconImage.color = Data.Color;
            iconImage.sprite = Data.Icon != null ? Data.Icon : iconImage.sprite;

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
            gameObject.SetActive(false);
        }
    }
}