using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class TaskInstanceTweenOffsetRectY : TaskInstanceTween
    {
        private readonly RectTransform transform = null;
        private Vector2 start = Vector2.zero;
        private Vector2 target = Vector2.zero;

        public TaskInstanceTweenOffsetRectY(RectTransform transform, float startValue, float targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.start = new(this.transform.anchoredPosition.x, startValue);
            this.target = new(this.transform.anchoredPosition.x, targetValue);
            this.transform.anchoredPosition = this.start;
        }

        protected override void OnFadeUpdate(float time)
        {
            start.x = transform.anchoredPosition.x;
            target.x = transform.anchoredPosition.x;
            transform.anchoredPosition = Vector2.Lerp(start, target, time);
        }
        protected override void OnFadeComplete() => transform.anchoredPosition = new(transform.anchoredPosition.x, target.y);

        public void OverrideStart(Vector2 value) => start = value;
        public void OverrideTarget(Vector2 value) => target = value;
    }
}
