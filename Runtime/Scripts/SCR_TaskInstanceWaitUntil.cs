using System;
using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitUntil: TaskInstance
    {
        private readonly Func<bool> predicate = default;
        private readonly Action callback = default;

        public TaskInstanceWaitUntil(MonoBehaviour host, Func<bool> predicate, Action callback) : base(host)
        {
            this.predicate = predicate ?? throw new NullReferenceException("TaskInstanceWaitUntil() predicate == null");
            this.callback = callback ?? throw new NullReferenceException("TaskInstanceWaitUntil() callback == null");
        }
        protected override void OnUpdate()
        {
            if (predicate.Invoke())
            {
                callback.Invoke();
                IsCompleted = true;
            }
        }
    }
}
