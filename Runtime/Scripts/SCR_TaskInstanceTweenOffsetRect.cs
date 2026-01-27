using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceTweenOffsetRect : TaskInstanceTween
    {
        private readonly RectTransform thisTransform = null;
        private readonly Vector2 startValue = Vector3.zero;
        private readonly Vector2 targetValue = Vector3.zero;

        public TaskInstanceTweenOffsetRect(RectTransform rectTransform, Vector2 startValue, Vector2 targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(rectTransform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.thisTransform = rectTransform;
            this.startValue = startValue;
            this.targetValue = targetValue;
        }

        protected override void OnFadeUpdate(float time) => thisTransform.anchoredPosition = Vector2.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisTransform.anchoredPosition = targetValue;
    }
}
