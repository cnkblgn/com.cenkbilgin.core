using System;
using UnityEngine;

namespace Core
{
    using Random = UnityEngine.Random;

    public class TaskInstanceWaitInterval : TaskInstance
    {
        private readonly Action callback;
        private readonly float minInterval;
        private readonly float maxInterval;
        private float interval = 0f;
        private float timer = 0f; 

        public TaskInstanceWaitInterval(Component host, float minInterval, float maxInterval, Action callback) : base(host)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.minInterval = Mathf.Max(0, minInterval);
            this.maxInterval = Mathf.Max(0, maxInterval);

            interval = Random.Range(this.minInterval, this.maxInterval);
        }
        protected override void OnUpdate()
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                callback.Invoke();

                interval = Random.Range(this.minInterval, this.maxInterval);
                timer = 0;
            }
        }
    }
}
