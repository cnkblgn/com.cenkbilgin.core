using UnityEngine;

namespace Core
{
    internal class TaskInstanceWaitSecondsRealtime<T> : TaskInstance where T : struct, ITaskCallback
    {
        private readonly T callback = default;
        private float time = 0;

        public TaskInstanceWaitSecondsRealtime(MonoBehaviour host, T callback, float duration) : base(host)
        {
            this.callback = callback;
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
