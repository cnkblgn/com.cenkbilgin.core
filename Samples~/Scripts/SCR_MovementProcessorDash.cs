using System;
using UnityEngine;
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementProcessorDash : MonoBehaviour, IMovementProcessor
    {
        public event Action OnStart = null;

        public int Priority => 4;

        [Header("_")]
        [SerializeField] private bool isAdditive = true;
        [SerializeField, Min(-1)] private int maxDashInAir = 1;

        [Header("_")]
        [SerializeField, Min(1)] private float force = 25f;
        [SerializeField, Min(0.1f)] private float cooldown = 2.5f;

        private MovementController movementController = null;
        private readonly StackBool isEnabled = new(8);
        private readonly InputActionType dashInput = new("Gameplay.Dash");
        private float dashTime = 0;
        private int dashCount = 0;

        private void Awake() => movementController = GetComponent<MovementController>();
        private void OnEnable() => movementController.OnLand += OnLand;
        private void OnDisable() => movementController.OnLand -= OnLand;

        private void OnLand(float arg1, float arg2, float arg3) => dashCount = Mathf.Max(0, dashCount - 1);
        public void OnBeforeMove(MovementController controller) 
        {
            if (!GetIsEnabled())
            {
                return;
            }

            if (maxDashInAir != -1 && !movementController.CollisionGround && dashCount >= maxDashInAir)
            {
                return;
            }

            if (dashInput.GetKeyDown())
            {
                if (Time.time - dashTime < cooldown)
                {
                    return;
                }

                Vector3 moveDirection = movementController.GetMovementDirectionWorld();
                Vector3 lookDirection = movementController.GetCharacterOrigin().forward;
                Vector3 velocity = moveDirection.sqrMagnitude > 0.01f ? moveDirection * force : lookDirection * force;

                if (isAdditive)
                {
                    movementController.AddVelocity(velocity);
                }
                else
                {
                    movementController.SetVelocity(velocity);
                }

                dashTime = Time.time;                
                OnStart?.Invoke();

                if (!movementController.CollisionGround)
                {
                    dashCount++;
                }
            }
        }
        public void OnBeforeLook(MovementController controller) { }

        public bool GetIsEnabled() => isEnabled.IsEnabled;
        public void Disable(out int token) => isEnabled.Disable(out token);
        public void Enable(ref int token) => isEnabled.Enable(ref token);
    }
}
