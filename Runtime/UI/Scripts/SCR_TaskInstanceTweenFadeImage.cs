using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class TaskInstanceTweenFadeImage : TaskInstanceTween
    {
        private readonly Image image = null;
        private Color start = COLOR_WHITE;
        private Color target = COLOR_WHITE;

        public TaskInstanceTweenFadeImage(Image image, Color targetColor, float targetAlpha, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.image = image;
            this.target = new(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
            this.start = this.image.color;
        }

        protected override void OnFadeUpdate(float time) => image.color = Color.Lerp(start, target, time);
        protected override void OnFadeComplete() => image.color = target;

        public void OverrideStart(Color value, float alpha) => start = new(value.r, value.g, value.b, alpha);
        public void OverrideTarget(Color value, float alpha) => target = new(value.r, value.g, value.b, alpha);
    }
}
