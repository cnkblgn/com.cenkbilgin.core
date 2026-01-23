using System;
using UnityEngine;

namespace Core
{
    public class TaskInstance
    {
        public bool IsCompleted { get; private set; } = false;

        private readonly MonoBehaviour host = null;
        private readonly Func<bool> predicate = null;
        private readonly Action onComplete = null;
        private float currentTime = 0;
        private readonly int targetFrame = 0;
        private readonly bool isRealtime = false;
        private readonly bool isFrametime = false;
        private readonly bool isPredicate = false;

        public TaskInstance(MonoBehaviour host, Func<bool> predicate, Action onComplete)
        {
            this.host = host;
            this.predicate = predicate;
            this.onComplete = onComplete;
            this.targetFrame = Time.frameCount + 1;
            this.currentTime = 0;

            this.isRealtime = false;
            this.isFrametime = false;
            this.IsCompleted = false;
            this.isPredicate = predicate != null;
        }
        public TaskInstance(MonoBehaviour host, Action onComplete, float waitSeconds, bool isRealtime, bool isFrametime)
        {
            this.host = host;
            this.predicate = null;
            this.onComplete = onComplete;

            this.targetFrame = Time.frameCount + 1;
            this.currentTime = waitSeconds;

            this.isRealtime = isRealtime;
            this.isFrametime = isFrametime;
            this.IsCompleted = false;
            this.isPredicate = false;
        }
        public void Update()
        {
            if (IsCompleted)
            {
                return;
            }

            if (host == null)
            {
                IsCompleted = true;
                return;
            }

            if (isFrametime && Time.frameCount > targetFrame)
            {
                onComplete?.Invoke();
                IsCompleted = true;
                return;
            }

            if (isPredicate)
            {
                if (predicate.Invoke())
                {
                    onComplete?.Invoke();
                    IsCompleted = true;
                    return;
                }

                return;
            }

            if (currentTime <= 0)
            {
                onComplete?.Invoke();
                IsCompleted = true;
            }

            currentTime -= isRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        public void Stop() => IsCompleted = true;
    }
}