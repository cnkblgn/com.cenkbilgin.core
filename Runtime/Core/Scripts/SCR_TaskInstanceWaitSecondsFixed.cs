using System;
using UnityEngine;

namespace Core
{
    public sealed class TaskInstanceWaitSecondsFixed : TaskInstance
    {
        private readonly Action callback = default;
        private float time = 0;

        public TaskInstanceWaitSecondsFixed(MonoBehaviour host, float duration, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException();
            this.time = duration;
        }
        protected override void OnUpdate()
        {
            time -= Time.fixedDeltaTime;

            if (time <= 0)
            {
                IsCompleted = true;
                callback.Invoke();
            }
        }
        public void OverrideDuration(float value) => time = value;
    }
}
