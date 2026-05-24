using System;
using UnityEngine;

namespace Core
{
    public sealed class TaskInstanceTick : TaskInstance
    {
        private readonly Action callback;

        public TaskInstanceTick(Component host, Action callback) : base(host) => this.callback = callback ?? throw new ArgumentNullException(nameof(callback));

        protected override void OnUpdate() => callback.Invoke();
    }
}
