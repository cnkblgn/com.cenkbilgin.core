using UnityEngine;
using Core.Input;
using System;

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
        [SerializeField, Min(1)] private float force = 25f;
        [SerializeField, Min(0.1f)] private float cooldown = 2.5f;

        private MovementController movementController = null;
        private readonly InputActionType dashInput = new("Gameplay.Dash");
        private float dashTime = 0;

        private void Awake() => movementController = GetComponent<MovementController>();

        public void OnBeforeMove(MovementController controller) 
        {
            if (dashInput.GetKeyDown())
            {
                if (Time.time - dashTime < cooldown)
                {
                    return;
                }

                Vector3 moveDirection = movementController.GetMovementDirectionWorld();
                Vector3 lookDirection = movementController.GetCharacterOrigin().forward;
                
                movementController.AddVelocity(moveDirection.sqrMagnitude > 0.01f ? moveDirection * force : lookDirection * force);

                dashTime = Time.time;
                OnStart?.Invoke();
            }
        }
        public void OnBeforeLook(MovementController controller) { }
    }
}
