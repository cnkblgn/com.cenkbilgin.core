using System;
using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitSecondsRealtime : TaskInstance
    {
        private readonly Action callback = default;
        private float time = 0;

        public TaskInstanceWaitSecondsRealtime(MonoBehaviour host, float duration, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitSecondsRealtime() callback == null");
            this.time = duration;
        }
        protected override void OnUpdate()
        {
            time -= Time.unscaledDeltaTime;

            if (time <= 0)
            {
                callback.Invoke();
                IsCompleted = true;
            }
        }
    }
}
