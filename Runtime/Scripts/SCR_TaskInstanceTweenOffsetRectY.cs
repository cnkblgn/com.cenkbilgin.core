using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceTweenOffsetRectY : TaskInstanceTween
    {
        private readonly RectTransform thisTransform = null;
        private Vector2 startValue = Vector2.zero;
        private Vector2 targetValue = Vector2.zero;

        public TaskInstanceTweenOffsetRectY(RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(rectTransform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            thisTransform = rectTransform;
            this.startValue = new(thisTransform.anchoredPosition.x, startValue);
            this.targetValue = new(thisTransform.anchoredPosition.x, targetValue);
            thisTransform.anchoredPosition = this.startValue;
        }

        protected override void OnFadeUpdate(float time)
        {
            startValue.x = thisTransform.anchoredPosition.x;
            targetValue.x = thisTransform.anchoredPosition.x;
            thisTransform.anchoredPosition = Vector2.Lerp(startValue, targetValue, time);
        }
        protected override void OnFadeComplete() => thisTransform.anchoredPosition = new(thisTransform.anchoredPosition.x, targetValue.y);
    }
}
