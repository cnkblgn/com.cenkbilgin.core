using System;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;
    using static TaskUtility;
    using static InputDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementModuleSlide : MonoBehaviour, IMovementModule
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

        private MovementController controller = null;
        private readonly StackBool isEnabled = new(8);
        private Vector3 slideDirection = Vector3.zero;
        private Vector3 slideVelocity = Vector3.zero;
        private float slideTimer = 0f;
        private int movementToken = 0;
        private bool isSliding = false;
        private bool canSlide = true;

        public void Bind(MovementController controller)
        {
            this.controller = controller;
        }
        public void Unbind(MovementController controller)
        {

        }

        public void OnBeforeMove() => UpdateSlide();
        public void OnBeforeLook() { }

        private void TryStartSlide()
        {
            if (!canSlide)
            {
                return;
            }

            if (!controller.CollisionGround)
            {
                return;
            }

            if (controller.IsOnSteepSlope)
            {
                return;
            }

            if (!controller.GetIsMovementEnabled())
            {
                return;
            }

            if (!Crouch.GetKey())
            {
                return;
            }

            float speed = controller.GetCurrentSpeed();

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

            if (!controller.CollisionGround)
            {
                EndSlide();
                return;
            }

            slideVelocity = controller.GetVelocity();
            RaycastHit ground = controller.GetGroundCollisionInfo();
            RaycastHit sides = controller.GetSidesCollisionInfo();
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

            if (!isMovingDownhill && slopeAngle > controller.GetSlopeLimit())
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

            if (!controller.CollisionSides)
            {
                slideVelocity = Vector3.ProjectOnPlane(slideVelocity, ground.normal);
            }
            else
            {
                slideVelocity = controller.CalculateClipVelocity(slideVelocity, sides.normal);
            }

            controller.SetVelocity(slideVelocity);
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

            slideDirection = controller.GetCharacterOrigin().forward.ClearY().normalized;
            slideVelocity = controller.GetCurrentSpeed() * slideDirection;

            slideVelocity = Vector3.ProjectOnPlane(slideVelocity, controller.GetGroundCollisionInfo().normal);

            controller.SetVelocity(slideVelocity);
            controller.DisableMovement(out movementToken);
            controller.OverrideMovementStance(MovementStance.CROUCH, true);

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

            controller.SetVelocity(slideVelocity);
            controller.EnableMovement(ref movementToken);
            controller.OverrideMovementStance(MovementStance.CROUCH, false);

            this.WaitSeconds(cooldown, () => canSlide = false, () => canSlide = true);
            OnEnd?.Invoke();
        }
        private void EjectSlide()
        {
            Vector3 groundNormal = controller.GetGroundCollisionInfo().normal;
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);

            slideVelocity += (groundNormal * Mathf.Lerp(1, 5, Mathf.Max(slopeAngle, 50) / 50));
            slideVelocity.y = controller.GetJumpForce();

            EndSlide();

            controller.RegisterJump();
        }

        public bool GetIsEnabled() => isEnabled.IsEnabled;
        public void Disable(out int token) => isEnabled.Disable(out token);
        public void Enable(ref int token) => isEnabled.Enable(ref token);
    }
}
