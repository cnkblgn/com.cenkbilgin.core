using System;
using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UINotificationElement : MonoBehaviour
    {
        public bool IsActive => isActive;
        public bool IsInitialized => isInitialized;

        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI notificationText = null;

        [Header("_")]
        [SerializeField] private Vector2 sizePadding = new(36, 0);

        [Header("_")]
        [SerializeField, Min(0)] private float offsetInDuration = 1;
        [SerializeField] private EaseType offsetInEaseType = EaseType.EASE_OUT_BOUNCE;
        [SerializeField, Min(0)] private float offsetOutDuration = 0.5f;
        [SerializeField] private EaseType offsetOutEaseType = EaseType.LINEAR;

        private RectTransform thisTransform = null;
        private TweenInstanceOffsetRectX thisTween = null;
        private Vector2 defaultPosition = Vector2.zero;
        private Vector2 defaultSize = Vector2.zero;
        private bool isInitialized = false;
        private bool isActive = false;

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            thisTransform = GetComponent<RectTransform>();
            thisTransform.AlignTopLeft();
            defaultPosition = thisTransform.anchoredPosition;
            defaultSize = thisTransform.sizeDelta;

            isInitialized = true;
        }
        public void Dispose()
        {
            thisTween?.Kill();
            isActive = false;

            thisTransform.anchoredPosition = defaultPosition;
            thisTransform.sizeDelta = defaultSize;

            gameObject.SetActive(false);
        }
        public void Show(string text, float duration)
        {
            gameObject.SetActive(true);

            isActive = true;
            thisTween?.Kill();

            notificationText.alignment = TextAlignmentOptions.MidlineLeft;
            notificationText.textWrappingMode = TextWrappingModes.NoWrap;
            notificationText.SetText(text);
            notificationText.ForceMeshUpdate();

            Vector2 textSize = notificationText.GetRenderedValues(false) + sizePadding;

            if (defaultSize.x < textSize.x)
            {
                thisTransform.sizeDelta = new(textSize.x, thisTransform.sizeDelta.y);
            }

            float startEndX = thisTransform.anchoredPosition.x;
            float startStartX = startEndX - (thisTransform.sizeDelta.x + 128);

            thisTween = thisTransform.OffsetX(startStartX, startEndX, offsetInDuration, duration, TweenType.SCALED, offsetInEaseType, () =>
            {
                float exitStartX = thisTransform.anchoredPosition.x;
                float exitEndX = startStartX;

                thisTween = thisTransform.OffsetX(exitStartX, exitEndX, offsetOutDuration, 0, TweenType.SCALED, offsetOutEaseType, Dispose);
            });
        }
        public void Offset(Vector2 offset) => thisTransform.anchoredPosition += offset;       
    }
}