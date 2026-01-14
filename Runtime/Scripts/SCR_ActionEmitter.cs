using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [DisallowMultipleComponent]
    public class ActionEmitter : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private UnityEvent actionEvent = null;
        [SerializeField, Min(0)] private float actionDelay = 0;

        public void Emit()
        {
            if (actionDelay <= 0)
            {
                EmitInternal();
                return;
            }

            this.WaitSeconds(null, EmitInternal, actionDelay, false);
        }
        private void EmitInternal() => actionEvent?.Invoke();
    }
}