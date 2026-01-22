using System;
using UnityEngine;

namespace Core
{
    public class TaskInstance
    {
        private readonly MonoBehaviour host = null;
        private readonly Func<bool> predicate = null;
        private readonly Action onComplete = null;
        private float currentTime = 0;
        private readonly int targetFrame = 0;
        private readonly bool isRealtime = false;
        private readonly bool isFrametime = false;
        private readonly bool isPredicate = false;
        public bool isFinished = false;

        public TaskInstance(MonoBehaviour host, Func<bool> predicate, Action onComplete)
        {
            this.host = host;
            this.predicate = predicate;
            this.onComplete = onComplete;
            this.isRealtime = false;
            this.isFrametime = false;
            targetFrame = Time.frameCount + 1;
            currentTime = 0;

            isFinished = false;
            isPredicate = predicate != null;
        }
        public TaskInstance(MonoBehaviour host, Action onComplete, float waitSeconds, bool isRealtime, bool isFrametime)
        {
            this.host = host;
            this.predicate = null;
            this.onComplete = onComplete;
            this.isRealtime = isRealtime;
            this.isFrametime = isFrametime;
            targetFrame = Time.frameCount + 1;
            currentTime = waitSeconds;

            isFinished = false;
            isPredicate = false;
        }
        public void Update()
        {
            if (isFinished)
            {
                return;
            }

            if (host == null)
            {
                isFinished = true;
                return;
            }

            if (isFrametime && Time.frameCount > targetFrame)
            {
                onComplete?.Invoke();
                isFinished = true;
                return;
            }

            if (isPredicate)
            {
                if (predicate.Invoke())
                {
                    onComplete?.Invoke();
                    isFinished = true;
                    return;
                }

                return;
            }

            if (currentTime <= 0)
            {
                onComplete?.Invoke();
                isFinished = true;
            }

            currentTime -= isRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
        public void Stop() => isFinished = true;
    }
}