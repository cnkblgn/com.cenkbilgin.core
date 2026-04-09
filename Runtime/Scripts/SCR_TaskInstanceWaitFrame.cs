using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceWaitFrame : TaskInstance
    {
        private readonly Action callback = default;
        private int frame = 0;

        public TaskInstanceWaitFrame(MonoBehaviour host, Action callback) : base(host)
        {
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitFrame() callback == null");
            this.frame = Time.frameCount + 1;
        }
        protected override void OnUpdate()
        {
            if (Time.frameCount > frame)
            {
                IsCompleted = true;
                callback.Invoke();
                return;
            }
        }

        public override void Reset()
        {
            base.Reset();

            this.frame = Time.frameCount + 1;
        }
    }
}
