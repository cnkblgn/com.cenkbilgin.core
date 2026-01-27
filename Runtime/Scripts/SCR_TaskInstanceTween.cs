using System;
using UnityEngine;

namespace Core
{
    public enum TweenType : byte { SCALED, UNSCALED }
    public abstract class TaskInstanceTween : TaskInstance
    {
        private readonly Action onComplete = null;
        private readonly EaseType easeType = EaseType.LINEAR;
        private readonly TweenType tweenType = TweenType.SCALED;
        private readonly float fadeSeconds = 0;
        private readonly float waitSeconds = 0;
        private float fadeTimer = 0;
        private float waitTimer = 0;
        private bool isFaded = false;

        public TaskInstanceTween(Component host, float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete) : base(host)
        {
            this.fadeSeconds = fadeSeconds;
            this.waitSeconds = waitSeconds;
            this.tweenType = tweenType;
            this.easeType = easeType;
            this.onComplete = onComplete;

            this.fadeTimer = 0;
            this.waitTimer = 0;
            this.isFaded = false;
        }

        protected sealed override void OnUpdate()
        {
            fadeTimer += tweenType == TweenType.SCALED ? Time.deltaTime : Time.unscaledDeltaTime;

            if (fadeTimer < fadeSeconds)
            {
                OnFadeUpdate(EaseEvaluater.Evaluate(easeType, fadeTimer / fadeSeconds));
                return;
            }

            if (!isFaded)
            {
                OnFadeComplete();
                isFaded = true;
            }

            waitTimer += tweenType == TweenType.SCALED ? Time.deltaTime : Time.unscaledDeltaTime;

            if (waitSeconds <= 0 || waitTimer >= waitSeconds)
            {
                Complete();
            }
        }
        public void Complete()
        {
            onComplete?.Invoke();
            Stop();
        }
        protected abstract void OnFadeUpdate(float time);
        protected abstract void OnFadeComplete();
    }
}
