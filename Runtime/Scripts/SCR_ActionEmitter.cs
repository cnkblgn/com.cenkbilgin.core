using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [DisallowMultipleComponent]
    public class ActionEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private bool emitOnStart = false;

        [Header("_")]
        [SerializeField] private UnityEvent actionEvent = null;
        [SerializeField, Min(0)] private float actionDelay = 0;

        [Header("_")]
        [SerializeField] private bool useInterval = false;
        [SerializeField] private float minInterval = 1;
        [SerializeField] private float maxInterval = 5;

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

            if (actionDelay <= 0)
            {
                callback();
                return;
            }

            this.WaitSeconds(actionDelay, callback);
        }
        private void EmitInternal() => actionEvent?.Invoke();
        private void StartInterval()
        {
            if (intervalTask != null)
            {
                return;
            }

            intervalTask = new TaskInstanceWaitInterval(this, minInterval, maxInterval, callback);
            TaskSystem.TryCreate(intervalTask);
        }
    }
}