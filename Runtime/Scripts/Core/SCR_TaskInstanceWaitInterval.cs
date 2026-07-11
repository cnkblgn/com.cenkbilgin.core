using System;
using UnityEngine;

namespace Core
{
    using Random = UnityEngine.Random;

    public sealed class TaskInstanceWaitInterval : TaskInstance
    {
        private readonly Action callback;
        private readonly float minInterval;
        private readonly float maxInterval;
        private readonly float duration;
        private float interval;
        private float intervalTime;
        private float totalTime;
        private readonly bool hasDuration;

        public TaskInstanceWaitInterval(Component host, float minInterval, float maxInterval, float duration, Action callback) : base(host)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.minInterval = Mathf.Max(0, minInterval);
            this.maxInterval = Mathf.Max(0, maxInterval);
            this.duration = duration;

            intervalTime = 0;
            totalTime = 0;
            interval = Random.Range(this.minInterval, this.maxInterval);

            hasDuration = duration > 0f;

            if (Mathf.Approximately(interval, 0)) throw new Exception("interval cannot be 0");
        }
        protected override void OnUpdate()
        {
            float deltaTime = Time.deltaTime;

            totalTime += deltaTime;

            if (hasDuration && totalTime >= duration)
            {
                IsCompleted = true;
                return;
            }

            intervalTime += deltaTime;

            if (intervalTime < interval)
            {
                return;
            }

            callback.Invoke();
            interval = Random.Range(minInterval, maxInterval);
            intervalTime = 0f;
        }
    }
}
