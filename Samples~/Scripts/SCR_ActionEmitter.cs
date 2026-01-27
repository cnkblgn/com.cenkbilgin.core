using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Misc
{
    using static TaskUtility;

    [DisallowMultipleComponent]
    public class ActionEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private UnityEvent actionEvent = null;
        [SerializeField, Min(0)] private float actionDelay = 0;

        private Action callback = default;

        private void Awake() => callback = EmitInternal;
        public void Emit()
        {
            if (actionDelay <= 0)
            {
                actionEvent?.Invoke();
                return;
            }

            this.WaitSeconds(actionDelay, callback);
        }
        private void EmitInternal() => actionEvent?.Invoke();
    }
}