using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    using static CoreUtility;

    public class TaskInstanceTweenFadeImage : TaskInstanceTween
    {
        private readonly Image thisImage = null;
        private Color startValue = COLOR_WHITE;
        private Color targetValue = COLOR_WHITE;

        public TaskInstanceTweenFadeImage(Image image, Color targetValue, float targetAlpha, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.thisImage = image;
            this.targetValue = new(targetValue.r, targetValue.g, targetValue.b, targetAlpha);
            this.startValue = this.thisImage.color;
        }

        protected override void OnFadeUpdate(float time) => thisImage.color = Color.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisImage.color = targetValue;

        public void OverrideStartValue(Color value, float alpha) => startValue = new(value.r, value.g, value.b, alpha);
        public void OverrideTargetValue(Color value, float alpha) => targetValue = new(value.r, value.g, value.b, alpha);
    }
}
