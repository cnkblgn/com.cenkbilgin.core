using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    using static CoreUtility;

    public class TaskInstanceTweenBlinkImage : TaskInstanceTween
    {
        private readonly Image thisImage = null;
        private Color startValue = COLOR_WHITE;
        private Color targetValue = COLOR_WHITE;
        private readonly float duration = 0;
        private float interval = 1;

        public TaskInstanceTweenBlinkImage(Image image, Color startValue, Color targetValue, float interval, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.thisImage = image;
            this.targetValue = targetValue;
            this.startValue = startValue;
            this.duration = fadeSeconds;
            this.interval = Mathf.Min(interval, duration, duration * 0.5f);
        }

        protected override void OnFadeUpdate(float _) => thisImage.SetCanvasColor(Color.Lerp(startValue, targetValue, Mathf.PingPong(Time.time / interval, 1f)));
        protected override void OnFadeComplete() { }

        public void OverrideInterval(float value) => this.interval = Mathf.Min(value, duration, duration * 0.5f);
        public void OverrideStartValue(Color value) => startValue = value;
        public void OverrideTargetValue(Color value) => targetValue = value;
    }
}
