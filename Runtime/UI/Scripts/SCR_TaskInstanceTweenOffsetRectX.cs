using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class TaskInstanceTweenOffsetRectX : TaskInstanceTween
    {
        private readonly RectTransform transform = null;
        private Vector2 start = Vector2.zero;
        private Vector2 target = Vector2.zero;

        public TaskInstanceTweenOffsetRectX(RectTransform transform, float start, float target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.start = new(start, this.transform.anchoredPosition.y);
            this.target = new(target, this.transform.anchoredPosition.y);
            this.transform.anchoredPosition = this.start;
        }

        protected override void OnFadeUpdate(float time)
        {
            start.y = transform.anchoredPosition.y;
            target.y = transform.anchoredPosition.y;
            transform.anchoredPosition = Vector2.Lerp(start, target, time);
        }
        protected override void OnFadeComplete() => transform.anchoredPosition = new(target.x, transform.anchoredPosition.y);

        public void OverrideStart(Vector2 value) => start = value;
        public void OverrideTarget(Vector2 value) => target = value;
    }
}
