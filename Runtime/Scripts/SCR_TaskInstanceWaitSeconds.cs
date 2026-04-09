using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceWaitSeconds : TaskInstance
    {
        private readonly Action callback = default;
        private float time = 0;

        public TaskInstanceWaitSeconds(MonoBehaviour host, float duration, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitSeconds() callback == null");
            this.time = duration;
        }
        protected override void OnUpdate()
        {
            time -= Time.deltaTime;

            if (time <= 0)
            {
                IsCompleted = true;
                callback.Invoke();
            }
        }

        public void OverrideDuration(float value) => time = value;
    }
}
