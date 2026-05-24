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
        public Vector3 Target => targetTransform != null ? targetTransform.position : targetPosition;
        public Sprite Icon => waypointImage.sprite;
        public Color Color => waypointImage.color;
        public bool IsCompleted { get; private set; }

        [Header("_")]
        [SerializeField, Required] private Image waypointImage = null;
        [SerializeField, Required] private TextMeshProUGUI waypointText = null;

        private RectTransform thisTransform = null;
        private Transform targetTransform = null;
        private Func<bool> destroyUntil = null;
        private Action completeCallback = null;
        private Vector3 targetPosition = Vector3.zero;
        private Vector3 targetOffset = Vector3.zero;
        private float tickDuration = -1;
        private float tickTimer = 0;
        private float cachedWidth = 0;
        private float cachedHeight = 0;
        private bool isInitialized = false;
        private bool isActive = false;
        private bool hasTarget = false;

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

            if (targetTransform == null && hasTarget)
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

            Vector2 position = cameraController.WorldToScreenPoint(Target + targetOffset);

            if (Vector3.Dot((Target - cameraTransform.position), cameraTransform.forward) < 0)
            {
                position.x *= -1;
            }

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            thisTransform.position = position;
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

        public void Show(Transform target, Vector3 offset, Sprite icon, Color color, string text, float duration, Func<bool> destroyUntil)
        {
            hasTarget = true;
            this.targetTransform = target;
            this.targetOffset = offset;
            Show(icon, color, text, duration, destroyUntil);
        }
        public void Show(Vector3 target, Sprite icon, Color color, string text, float duration, Func<bool> destroyUntil)
        {
            hasTarget = false;
            targetPosition = target;
            Show(icon, color, text, duration, destroyUntil);
        }
        private void Show(Sprite icon, Color color, string text, float duration, Func<bool> destroyUntil)
        {
            if (!isInitialized)
            {
                return;
            }

            gameObject.SetActive(true);

            isActive = true;
            IsCompleted = false;
            tickTimer = 0;

            this.tickDuration = duration;
            this.destroyUntil = destroyUntil;

            waypointText.text = text;
            waypointImage.color = color;
            waypointImage.sprite = icon != null ? icon : waypointImage.sprite;

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