using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class TaskInstanceTweenFadeCanvas : TaskInstanceTween
    {
        private readonly CanvasGroup canvas = null;
        private float start = 0;
        private float target = 0;

        public TaskInstanceTweenFadeCanvas(CanvasGroup canvasGroup, float target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(canvasGroup, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            canvas = canvasGroup;
            this.target = target;
            this.start = this.canvas.alpha;
        }

        protected override void OnFadeUpdate(float time) => canvas.alpha = Mathf.Lerp(start, target, time);
        protected override void OnFadeComplete() => canvas.alpha = target;

        public void OverrideStart(float value) => start = value;
        public void OverrideTarget(float value) => target = value;
    }
}
