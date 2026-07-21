using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class TaskInstanceTweenOffsetRect : TaskInstanceTween
    {
        private readonly RectTransform transform = null;
        private Vector2 start = Vector3.zero;
        private Vector2 target = Vector3.zero;

        public TaskInstanceTweenOffsetRect(RectTransform transform, Vector2 start, Vector2 target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(transform, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.transform = transform;
            this.start = start;
            this.target = target;
        }

        protected override void OnFadeUpdate(float time) => transform.anchoredPosition = Vector2.Lerp(start, target, time);
        protected override void OnFadeComplete() => transform.anchoredPosition = target;

        public void OverrideStart(Vector2 value) => start = value;
        public void OverrideTarget(Vector2 value) => target = value;
    }
}
