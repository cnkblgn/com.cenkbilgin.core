using System;
using UnityEngine;

namespace Core
{
    public enum TweenType : byte { SCALED, UNSCALED }
    public abstract class TaskInstanceTween : TaskInstance
    {
        private readonly Action onComplete;
        private readonly EaseType easeType;
        private readonly TweenType tweenType;
        private readonly float fadeSeconds;
        private readonly float waitSeconds;
        private float fadeTimer;
        private float waitTimer;
        private bool isFaded;

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
            IsCompleted = true;
            onComplete?.Invoke();
        }
        public override void Reset()
        {
            base.Reset();

            fadeTimer = 0;
            waitTimer = 0;
            isFaded = false;
        }

        protected abstract void OnFadeUpdate(float time);
        protected abstract void OnFadeComplete();
    }
}
