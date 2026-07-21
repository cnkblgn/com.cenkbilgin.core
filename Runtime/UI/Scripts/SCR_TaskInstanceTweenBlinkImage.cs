using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class TaskInstanceTweenBlinkImage : TaskInstanceTween
    {
        private readonly Image thisImage = null;
        private Color start = COLOR_WHITE;
        private Color target = COLOR_WHITE;
        private readonly float duration = 0;
        private float interval = 1;

        public TaskInstanceTweenBlinkImage(Image image, Color start, Color target, float interval, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(image, fadeSeconds, waitSeconds, tweenType, easeType, onComplete)
        {
            this.thisImage = image;
            this.target = target;
            this.start = start;
            this.duration = fadeSeconds;
            this.interval = Mathf.Min(interval, duration, duration * 0.5f);
        }

        protected override void OnFadeUpdate(float _) => thisImage.SetCanvasColor(Color.Lerp(start, target, Mathf.PingPong(Time.time / interval, 1f)));
        protected override void OnFadeComplete() { }

        public void OverrideInterval(float value) => this.interval = Mathf.Min(value, duration, duration * 0.5f);
        public void OverrideStart(Color value) => start = value;
        public void OverrideTarget(Color value) => target = value;
    }
}
