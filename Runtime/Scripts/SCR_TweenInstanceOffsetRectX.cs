using System;
using UnityEngine;

namespace Core
{
    public class TweenInstanceOffsetRectX : TweenInstance
    {
        protected override bool CanUpdate => thisTransform != null;

        private readonly RectTransform thisTransform = null;
        private Vector2 startValue = Vector2.zero;
        private Vector2 targetValue = Vector2.zero;

        public TweenInstanceOffsetRectX(RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException("TweenInstanceOffsetRectX() << " + nameof(rectTransform));
            }

            thisTransform = rectTransform;
            this.startValue = new(startValue, thisTransform.anchoredPosition.y);
            this.targetValue = new(targetValue, thisTransform.anchoredPosition.y);
            thisTransform.anchoredPosition = this.startValue;
        }

        protected override void OnFadeUpdate(float time)
        {
            startValue.y = thisTransform.anchoredPosition.y;
            targetValue.y = thisTransform.anchoredPosition.y;
            thisTransform.anchoredPosition = Vector2.Lerp(startValue, targetValue, time);
        }
        protected override void OnFadeComplete() => thisTransform.anchoredPosition = new(targetValue.x, thisTransform.anchoredPosition.y);
    }
}
