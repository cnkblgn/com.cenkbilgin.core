using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitUntil<T1, T2> : TaskInstance where T1 : struct, ITaskPredicate where T2 : struct, ITaskCallback
    {
        private readonly T1 predicate = default;
        private readonly T2 callback = default;

        public TaskInstanceWaitUntil(MonoBehaviour host, T1 predicate, T2 callback) : base(host)
        {
            this.predicate = predicate;
            this.callback = callback;
        }
        protected override void OnUpdate()
        {
            if (predicate.Evaluate())
            {
                callback.Invoke();
                IsCompleted = true;
            }
        }
    }
}
