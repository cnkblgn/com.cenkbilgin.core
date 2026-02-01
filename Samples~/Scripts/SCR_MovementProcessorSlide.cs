using System;
using UnityEngine;
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;
    using static TaskUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementProcessorSlide : MonoBehaviour, IMovementProcessor
    {
        public event Action OnStart = null;
        public event Action OnEnd = null;

        public int Priority => 1;
        public bool IsSliding => isSliding;
        
        [Header("_")]
        [SerializeField, Min(0)] private float cooldown = 0.5f;
        [SerializeField, Min(1)] private float baseSpeed = 1f;
        [SerializeField, Min(1)] private float maxSpeed = 50f;
        [SerializeField, Min(1)] private float minSpeed = 4f;
        [SerializeField, Min(1)] private float acceleration = 8f;

        private MovementController movementController = null;
        private readonly StackBool isEnabled = new();
        private Vector3 slideDirection = Vector3.zero;
        private Vector3 slideVelocity = Vector3.zero;
        private float slideTimer = 0f;
        private bool isSliding = false;
        private bool canSlide = true;

        private void Awake() => movementController = GetComponent<MovementController>();

        public void OnBeforeMove(MovementController controller) => UpdateSlide();
        public void OnBeforeLook(MovementController controller) { }

        private void TryStartSlide()
        {
            if (!canSlide)
            {
                return;
            }

            if (!movementController.CollisionGround)
            {
                return;
            }

            if (movementController.IsOnSteepSlope)
            {
                return;
            }

            if (!movementController.GetIsMovementEnabled())
            {
                return;
            }

            if (!Crouch.GetKey())
            {
                return;
            }

            float speed = movementController.GetCurrentSpeed();

            if (speed < minSpeed)
            {
                return;
            }

            StartSlide();
        }
        private void UpdateSlide()
        {
            if (!isSliding)
            {
                TryStartSlide();
                return;
            }

            if (!movementController.CollisionGround)
            {
                EndSlide();
                return;
            }

            slideVelocity = movementController.GetVelocity();
            RaycastHit ground = movementController.GetGroundCollisionInfo();
            RaycastHit sides = movementController.GetSidesCollisionInfo();
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, ground.normal).normalized;
            float slopeAngle = Vector3.Angle(ground.normal, Vector3.up);

            if (Jump.GetKeyDown())
            {
                EjectSlide();
                return;
            }

            if (slideVelocity.magnitude <= 1)
            {
                EndSlide();
                return;
            }

            bool isMovingDownhill = Vector3.Dot(slideVelocity, slopeDirection) > 0f;

            if (!isMovingDownhill && slopeAngle > movementController.GetSlopeLimit())
            {
                isSliding = false;
                return;
            }

            if (isMovingDownhill)
            {
                slideTimer += Time.deltaTime;
            }

            float timeBoost = Mathf.Min(slideTimer * 0.1f, 2f);
            float slopeBoost = slopeAngle * 2f;
            float targetSpeed = isMovingDownhill ? Mathf.Clamp(baseSpeed + slopeBoost + timeBoost, 0f, maxSpeed) : 0f;

            slideVelocity = slideVelocity.normalized * Mathf.MoveTowards(slideVelocity.magnitude, targetSpeed, acceleration * Time.deltaTime);

            if (!movementController.CollisionSides)
            {
                slideVelocity = Vector3.ProjectOnPlane(slideVelocity, ground.normal);
            }
            else
            {
                slideVelocity = movementController.CalculateClipVelocity(slideVelocity, sides.normal);
            }

            movementController.SetVelocity(slideVelocity);
        }
        private void StartSlide()
        {
            if (isSliding)
            {
                return;
            }

            if (!GetIsEnabled())
            {
                return;
            }

            isSliding = true;
            slideTimer = 0f;

            slideDirection = movementController.GetCharacterOrigin().forward.ClearY().normalized;
            slideVelocity = movementController.GetCurrentSpeed() * slideDirection;

            movementController.SetVelocity(slideVelocity);
            movementController.SetIsMovementEnabled(false);
            movementController.OverrideMovementStance(MovementStance.CROUCH, true);

            OnStart?.Invoke();
        }
        private void EndSlide()
        {
            if (!isSliding)
            {
                return;
            }

            isSliding = false;
            slideTimer = 0f;

            movementController.SetVelocity(slideVelocity);
            movementController.SetIsMovementEnabled(true);
            movementController.OverrideMovementStance(MovementStance.CROUCH, false);

            this.WaitSeconds(cooldown, () => canSlide = false, () => canSlide = true);
            OnEnd?.Invoke();
        }
        private void EjectSlide()
        {
            Vector3 groundNormal = movementController.GetGroundCollisionInfo().normal;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            slideVelocity += (groundNormal * Mathf.Lerp(1, 5, Mathf.Max(slopeAngle, 50) / 50));
            slideVelocity.y = movementController.GetJumpForce();

            EndSlide();

            movementController.RegisterJump();
        }

        public bool GetIsEnabled() => isEnabled.IsEnabled;
        public void SetIsEnabled(bool value)
        {
            if (value)
            {
                isEnabled.Enable();
            }
            else
            {
                EndSlide();
                isEnabled.Disable();
            }
        }
    }
}
