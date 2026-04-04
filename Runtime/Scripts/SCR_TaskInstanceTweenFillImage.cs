using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    using static CoreUtility;

    public class TaskInstanceTweenFillImage : TaskInstanceTween
    {
        private readonly Image thisImage = null;
        private readonly float startValue = 0;
        private readonly float targetValue = 1;

        public TaskInstanceTweenFillImage(Image image, float targetValue, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.thisImage = image;
            this.targetValue = targetValue;
            this.startValue = image.fillAmount;
        }

        protected override void OnFadeUpdate(float time) => thisImage.fillAmount = Mathf.Lerp(startValue, targetValue, time);
        protected override void OnFadeComplete() => thisImage.fillAmount = targetValue;
    }
}
