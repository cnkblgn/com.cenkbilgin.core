using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public sealed class TaskInstanceTweenFillImage : TaskInstanceTween
    {
        private readonly Image image = null;
        private float start = 0;
        private float target = 1;

        public TaskInstanceTweenFillImage(Image image, float target, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.image = image;
            this.target = target;
            this.start = image.fillAmount;
        }

        protected override void OnFadeUpdate(float time) => image.fillAmount = Mathf.Lerp(start, target, time);
        protected override void OnFadeComplete() => image.fillAmount = target;

        public void OverrideStart(float value) => start = value;
        public void OverrideTarget(float value) => target = value;
    }
}
