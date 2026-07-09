using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [DisallowMultipleComponent]
    public sealed class EventEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool emitOnStart = false;

        [Header("_")]
        [SerializeField] private UnityEvent @event = null;
        [SerializeField, Min(0)] private float delay = 0;

        [Header("_")]
        [SerializeField] private bool useInterval = false;
        [SerializeField] private float minInterval = 1;
        [SerializeField] private float maxInterval = 5;
        [SerializeField] private float duration = 0;

        private TaskInstanceWaitInterval intervalTask = null;
        private Action callback = default;

        private void Awake() => callback = EmitInternal;
        private void Start()
        {
            if (emitOnStart)
            {
                Emit();
            }
        }

        public void Emit()
        {
            if (useInterval)
            {
                StartInterval();
                return;
            }

            if (delay <= 0)
            {
                callback();
                return;
            }

            this.WaitSeconds(delay, callback);
        }
        private void EmitInternal() => @event?.Invoke();

        private void StartInterval()
        {
            if (intervalTask != null)
            {
                return;
            }

            intervalTask = new TaskInstanceWaitInterval(this, minInterval, maxInterval, duration, callback);
            TaskSystem.TryCreate(intervalTask);
        }
    }
}