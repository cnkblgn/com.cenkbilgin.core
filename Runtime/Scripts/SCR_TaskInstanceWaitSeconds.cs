using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitSeconds<T> : TaskInstance where T : struct, ITaskCallback
    {
        private readonly T callback = default;
        private float time = 0;

        public TaskInstanceWaitSeconds(MonoBehaviour host, T callback, float duration) : base(host)
        {
            this.callback = callback;
            this.time = duration;
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
    }
}
