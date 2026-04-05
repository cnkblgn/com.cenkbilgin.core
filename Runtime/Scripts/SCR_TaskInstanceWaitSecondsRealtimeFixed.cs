using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceWaitSecondsRealtimeFixed : TaskInstance
    {
        private readonly Action callback = default;
        private float time = 0;

        public TaskInstanceWaitSecondsRealtimeFixed(MonoBehaviour host, float duration, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitSecondsRealtimeFixed() callback == null");
            this.time = duration;
        }
        protected override void OnUpdate()
        {
            time -= Time.fixedUnscaledDeltaTime;

            if (time <= 0)
            {
                IsCompleted = true;
                callback.Invoke();
            }
        }
        public void OverrideDuration(float value) => time = value;
    }
}
