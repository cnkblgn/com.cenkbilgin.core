using System;
using UnityEngine;

namespace Core
{
    public class TaskInstanceWaitUntil: TaskInstance
    {
        private readonly Func<bool> predicate = default;
        private readonly Action callback = default;

        public TaskInstanceWaitUntil(MonoBehaviour host, Func<bool> predicate, Action callback) : base(host)
        {
            this.predicate = predicate ?? throw new NullReferenceException();
            this.callback = callback ?? throw new NullReferenceException();
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
