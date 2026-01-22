using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public class UIPointerEventScale : UIPointerEvent
    {
        [Header("_")]
        [SerializeField] private EaseType onClickScaleEasingType = EaseType.LINEAR;
        [SerializeField, Min(0.01f)] private float onClickScaleDuration = 0.035f;
        [SerializeField, Min(0.01f)] private float onClickScaleDelay = 0.1f;
        [SerializeField, Range(-1.0f, 1.0f)] private float onClickScalePower = 0.125f;

        [Header("_")]
        [SerializeField] private EaseType onHoverScaleEasingType = EaseType.LINEAR;
        [SerializeField, Min(0.01f)] private float onHoverScaleDuration = 0f;
        [SerializeField, Min(0.01f)] private float onHoverScaleDelay = 0f;
        [SerializeField, Range(-1.0f, 1.0f)] private float onHoverScalePower = 0f;

        private RectTransform thisTransform = null;
        private TweenInstanceScaleTransform thisTween = null;
        private Vector3 defaultScale = Vector3.one;

        private void Awake()
        {
            thisTransform = GetComponent<RectTransform>();
            defaultScale = thisTransform.localScale;
        }
        private void OnEnable() => thisTransform.localScale = defaultScale;

        protected override void OnSubmitInternal(BaseEventData eventData)
        {
            base.OnSubmitInternal(eventData);

            Scale(onClickScalePower, onClickScaleDelay, onClickScaleDuration, onClickScaleEasingType);
        }
        protected override void OnSelectInternal(BaseEventData eventData)
        {
            base.OnSelectInternal(eventData);

            Scale(onHoverScalePower, onHoverScaleDelay, onHoverScaleDuration, onHoverScaleEasingType);
        }
        protected override void OnPointerClickInternal(PointerEventData eventData)
        {
            base.OnPointerClickInternal(eventData);

            Scale(onClickScalePower, onClickScaleDelay, onClickScaleDuration, onClickScaleEasingType);
        }
        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            Scale(onHoverScalePower, onHoverScaleDelay, onHoverScaleDuration, onHoverScaleEasingType);
        }

        private void Scale(float scalePower, float scaleDelay, float scaleDuration, EaseType easeType)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (scalePower == 0)
            {
                return;
            }

            thisTween?.Kill();

            Vector3 startValue = defaultScale;
            Vector3 endValue = startValue - (startValue * scalePower);

            thisTransform.localScale = startValue;

            thisTween = thisTransform.Scale(endValue, scaleDuration, scaleDelay, TweenType.UNSCALED, easeType, () => thisTween = thisTransform.Scale(defaultScale, scaleDuration, 0, TweenType.UNSCALED, easeType) );
        }
    }
}