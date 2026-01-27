using System;
using UnityEngine;

namespace Core
{
    public abstract class TaskInstance
    {
        public bool IsCompleted { get; protected set; }

        private readonly Component host = null;
        public TaskInstance(Component host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("TaskInstance() host == null"); 
            }

            this.host = host;
        }

        internal void Update()
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