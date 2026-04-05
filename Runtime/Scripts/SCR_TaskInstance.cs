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

        /// <summary> WARNING: Only use this if necessary! This belongs to TaskSystem but if you run locally then you can call this Update </summary>
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
        public void Reset() => IsCompleted = false;
        public void Stop() => IsCompleted = true;
        protected abstract void OnUpdate();
    }
}