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

        private InvokeEvent callback = default;

        private void Awake() => callback = new(actionEvent);
        public void Emit()
        {
            if (actionDelay <= 0)
            {
                callback.Invoke();
                return;
            }

            this.WaitSeconds(actionDelay, callback);
        }
    }
}