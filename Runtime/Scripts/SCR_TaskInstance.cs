using UnityEngine;

namespace Core
{
    public interface ITaskPredicate { public bool Evaluate(); }
    public interface ITaskCallback { public void Invoke(); }

    public abstract class TaskInstance
    {
        public bool IsCompleted { get; protected set; }

        private readonly MonoBehaviour host = null;
        public TaskInstance(MonoBehaviour host) => this.host = host;

        public void Update()
        {
            if (IsCompleted)
            {
                return;
            }

            if (host == null)
            {
                IsCompleted = true;
                return;
            }

            OnUpdate();
        }
        public void Stop() => IsCompleted = true;
        protected abstract void OnUpdate();
    }
}