using System;
using UnityEngine;

namespace Core
{
    public abstract class TweenInstance
    {
        public bool IsCompleted => isBaseCompleted;
        protected abstract bool CanUpdate { get; }

        private readonly Action onComplete = null;
        private readonly EaseType easeType;
        private readonly TweenType tweenType;
        private readonly float fadeSeconds = 0;
        private readonly float waitSeconds = 0;
        private float fadeTimer = 0;
        private float waitTimer = 0;
        private bool isFadeCompleted = false;
        private bool isBaseCompleted = false;

        public TweenInstance(float fadeSeconds, float waitSeconds, TweenType tweenType, EaseType easeType, Action onComplete)
        {
            this.fadeSeconds = fadeSeconds;
            this.waitSeconds = waitSeconds;
            this.onComplete = onComplete;
            this.tweenType = tweenType;
            this.easeType = easeType;
        }
        public void Update()
        {
            if (!CanUpdate)
            {
                isBaseCompleted = true;
                return;
            }

            if (isBaseCompleted)
            {
                return;
            }

            fadeTimer += tweenType == TweenType.SCALED ? Time.deltaTime : Time.unscaledDeltaTime;

            if (fadeTimer < fadeSeconds)
            {
                OnFadeUpdate(EaseEvaluater.Evaluate(easeType, fadeTimer / fadeSeconds));
            }
            else
            {
                if (!isFadeCompleted)
                {
                    OnFadeComplete();
                    isFadeCompleted = true;
                }

                waitTimer += tweenType == TweenType.SCALED ? Time.deltaTime : Time.unscaledDeltaTime;

                if (waitSeconds <= 0 || waitTimer >= waitSeconds)
                {
                    Complete();
                }
            }
        }
        public void Complete() { onComplete?.Invoke(); Kill(); }
        public void Kill() => isBaseCompleted = true;
        protected abstract void OnFadeUpdate(float time);
        protected abstract void OnFadeComplete();
    }
}
