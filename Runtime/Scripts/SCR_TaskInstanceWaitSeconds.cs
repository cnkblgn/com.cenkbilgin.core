using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceWaitSeconds: TaskInstance
    {
        private Action callback = default;
        private float time = 0;

        public TaskInstanceWaitSeconds(MonoBehaviour host, float duration, Action callback) : base(host)
        {
            SetCallback(callback);
            SetDuration(duration);
        }
        protected override void OnUpdate()
        {
            time -= Time.deltaTime;

            if (time <= 0)
            {
                callback.Invoke();
                IsCompleted = true;
            }
        }

        public void SetCallback(Action callback)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitSeconds() callback == null");
        }
        public void SetDuration(float duration)
        {
            this.time = duration;
        }
    }
}
