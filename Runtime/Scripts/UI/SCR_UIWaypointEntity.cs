using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIWaypointEntity : MonoBehaviour
    {
        public UIWaypointData Data { get; private set; }
        public bool IsCompleted { get; private set; }

        [Header("_")]
        [SerializeField, Required] private Image waypointImage = null;
        [SerializeField, Required] private TextMeshProUGUI waypointText = null;

        private RectTransform thisTransform = null;
        private Func<bool> destroyUntil = null;
        private Action completeCallback = null;
        private Vector3 offset = Vector3.zero;
        private float tickDuration = -1;
        private float tickTimer = 0;
        private float cachedWidth = 0;
        private float cachedHeight = 0;
        private bool isInitialized = false;
        private bool isActive = false;

        public void Tick(Camera cameraController, Transform cameraTransform)
        {
            if (!isActive)
            {
                return;
            }

            if (cameraController == null)
            {
                Complete();
                return; 
            }

            if (cameraTransform == null)
            {
                Complete();
                return;
            }

            if (Data.HasTarget && Data.TargetTransform == null)
            {
                Complete();
                return;
            }

            if (destroyUntil != null && destroyUntil())
            {
                Hide();
                return;
            }

            if (tickDuration >= 1)
            {
                if (tickTimer > tickDuration)
                {
                    Hide();
                    return;
                }

                tickTimer += Time.deltaTime;
            }

            float minX = cachedWidth;
            float maxX = Screen.width - minX;

            float minY = cachedHeight;
            float maxY = Screen.height - minY;

            Vector3 worldPosition = Data.Position + offset;
            Vector2 screenPosition = cameraController.WorldToScreenPoint(worldPosition);

            if (Vector3.Dot((worldPosition - cameraTransform.position), cameraTransform.forward) < 0)
            {
                screenPosition.x *= -1;
            }

            screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
            screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);
            thisTransform.position = screenPosition;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            thisTransform = GetComponent<RectTransform>();
            completeCallback = Complete;

            isInitialized = true;
        }
        public void Deinitialize()
        {
            if (!isInitialized)
            {
                return;
            }

            Complete();

            isActive = false;
            IsCompleted = false;
        }

        public void Show(UIWaypointData data, Vector3 offset, Func<bool> destroyUntil)
        {
            if (!isInitialized)
            {
                return;
            }

            Data = data;

            gameObject.SetActive(true);

            isActive = true;
            IsCompleted = false;
            tickTimer = 0;

            tickDuration = Data.Duration;
            this.offset = offset;
            this.destroyUntil = destroyUntil;

            waypointText.text = Data.Text;
            waypointImage.color = Data.Color;
            waypointImage.sprite = Data.Icon != null ? Data.Icon : waypointImage.sprite;

            thisTransform.localScale = Vector3.zero;
            thisTransform.Scale(Vector3.one, 0.25f);

            cachedWidth = LayoutUtility.GetPreferredWidth(thisTransform) * 0.5f;
            cachedHeight = LayoutUtility.GetPreferredHeight(thisTransform) * 0.5f;
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