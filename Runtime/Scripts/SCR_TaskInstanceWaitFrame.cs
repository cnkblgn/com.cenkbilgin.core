using UnityEngine;

namespace Core
{

    internal class TaskInstanceWaitFrame<T> : TaskInstance where T : struct, ITaskCallback
    {
        private readonly T callback = default;
        private readonly int frame = 0;

        public TaskInstanceWaitFrame(MonoBehaviour host, T callback) : base(host)
        {
            this.callback = callback;
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
