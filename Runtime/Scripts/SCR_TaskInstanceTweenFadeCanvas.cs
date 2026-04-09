using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceTweenFadeCanvas : TaskInstanceTween
    {
        private readonly CanvasGroup thisCanvas = null;
        private float startValue = 0;
        private float targetValue = 0;

        public TaskInstanceTweenFadeCanvas(CanvasGroup canvasGroup, float targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(canvasGroup, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            thisCanvas = canvasGroup;
            this.targetValue = targetValue;
            this.startValue = this.thisCanvas.alpha;
        }

        protected override void OnFadeUpdate(float time) => thisCanvas.alpha = Mathf.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisCanvas.alpha = targetValue;

        public void OverrideStartValue(float value) => startValue = value;
        public void OverrideTargetValue(float value) => targetValue = value;
    }
}
