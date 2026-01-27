using System;
using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitFrame : TaskInstance
    {
        private readonly Action callback = default;
        private readonly int frame = 0;

        public TaskInstanceWaitFrame(MonoBehaviour host, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitFrame() callback == null");
            this.frame = Time.frameCount + 1;
        }
        protected override void OnUpdate()
        {
            if (Time.frameCount > frame)
            {
                callback.Invoke();
                IsCompleted = true;
                return;
            }
        }
    }
}
