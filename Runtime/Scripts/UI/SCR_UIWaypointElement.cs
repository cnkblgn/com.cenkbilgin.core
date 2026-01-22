using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIWaypointElement : MonoBehaviour
    {
        public bool IsFinished => isFinished; 

        [Header("_")]
        [SerializeField, Required] private Image waypointImage = null;
        [SerializeField, Required] private TextMeshProUGUI waypointText = null;
       
        private RectTransform thisTransform = null;
        private Transform targetTransform = null;
        private Func<bool> destroyUntil = null;
        private Vector3 targetOffset = Vector3.zero;
        private float tickDuration = -1;
        private float tickTimer = 0;
        private bool isFinished = false;

        public void Tick(Camera mainCameraController, Transform mainCameraTransform)
        {
            if (mainCameraController == null)
            {
                Deinitialize();
                return;
            }

            if (mainCameraTransform == null)
            {
                Deinitialize();
                return;
            }

            if (destroyUntil != null && destroyUntil())
            {
                Deinitialize();
                return;
            }

            if (targetTransform == null)
            {
                Deinitialize();
                return;
            }

            if (tickDuration >= 1)
            {
                if (tickTimer > tickDuration)
                {
                    Deinitialize();
                    return;
                }

                tickTimer += Time.deltaTime;
            }

            float minX = LayoutUtility.GetPreferredWidth(thisTransform) / 2;
            float maxX = Screen.width - minX;

            float minY = LayoutUtility.GetPreferredHeight(thisTransform) / 2;
            float maxY = Screen.height - minY;

            Vector2 position = mainCameraController.WorldToScreenPoint(targetTransform.position + targetOffset);

            if (Vector3.Dot((targetTransform.position - mainCameraTransform.position), mainCameraTransform.forward) < 0)
            {
                position.x = position.x < Screen.width / 2 ? maxX : minX;
            }

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
            thisTransform.position = position;
        }
        public UIWaypointElement Initialize(Transform targetTransform, Vector3 targetOffset, Func<bool> destroyUntil, Sprite iconSprite, Color iconColor, string iconText = "", float duration = -1)
        {
            thisTransform = GetComponent<RectTransform>();
            this.targetTransform = targetTransform;
            this.targetOffset = targetOffset;
            this.tickDuration = duration;
            this.destroyUntil = destroyUntil;

            waypointText.text = iconText;
            waypointImage.color = iconColor;
            waypointImage.sprite = iconSprite != null ? iconSprite : waypointImage.sprite;

            thisTransform.localScale = Vector3.zero;
            thisTransform.Scale(Vector3.one, 0.25f);

            return this;
        }
        public void Deinitialize()
        {
            isFinished = true;
            thisTransform.Scale(Vector3.zero, 0.25f, 0.25f, TweenType.SCALED, EaseType.LINEAR, () => Destroy(this.gameObject));
        }
    }
}