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
        private float interval;
        private float timer;
        private readonly bool instant;

        public TaskInstanceWaitInterval(Component host, float minInterval, float maxInterval, Action callback) : base(host)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.minInterval = Mathf.Max(0, minInterval);
            this.maxInterval = Mathf.Max(0, maxInterval);

            timer = 0;
            interval = Random.Range(this.minInterval, this.maxInterval);
            instant = Mathf.Approximately(interval, 0);
        }
        protected override void OnUpdate()
        {
            timer += Time.deltaTime;

            if (instant)
            {
                callback.Invoke();
            } 
            else if (timer >= interval)
            {
                callback.Invoke();

                interval = Random.Range(this.minInterval, this.maxInterval);
                timer = 0;
            }
        }
    }
}
