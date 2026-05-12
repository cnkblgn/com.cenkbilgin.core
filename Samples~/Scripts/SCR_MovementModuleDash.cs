using System;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementModuleDash : MonoBehaviour, IMovementModule
    {
        public event Action OnStart = null;

        public int Priority => 4;

        [Header("_")]
        [SerializeField, Min(-1)] private int maxDashInAir = 1;

        [Header("_")]
        [SerializeField] private bool isAdditive = true;
        [SerializeField] private bool disableFriction = false;
        [SerializeField] private bool disableGravity = false;
        [SerializeField, Min(1)] private float force = 25f;
        [SerializeField, Min(0.1f)] private float cooldown = 2.5f;

        private MovementController controller = null;
        private readonly StackBool isEnabled = new(8);
        private TaskInstance frictionTask = null;
        private TaskInstance gravityTask = null;
        private Action overrideFriction = null;
        private Action overrideGravity = null;
        private Action resetFriction = null;
        private Action resetGravity = null;
        private static readonly InputActionType dashInput = new("Gameplay.Dash");
        private float dashFriction = 0;
        private float dashGravity = 0;
        private float dashTime = 0;
        private int dashCount = 0;
        private bool isFrictionOverriden = false;
        private bool isGravityOverriden = false;

        public void Bind(MovementController controller)
        {
            this.controller = controller;

            overrideFriction = OverrideFriction;
            overrideGravity = OverrideGravity;

            resetFriction = ResetFriction;
            resetGravity = ResetGravity;

            this.controller.OnLand += OnLand;
        }
        public void Unbind(MovementController controller)
        {
            this.controller.OnLand -= OnLand;
        }

        private void OnLand(float arg1, float arg2, float arg3) => dashCount = Mathf.Max(0, dashCount - 1);
        public void OnBeforeMove() 
        {
            if (!GetIsEnabled())
            {
                return;
            }

            if (maxDashInAir != -1 && !controller.CollisionGround && dashCount >= maxDashInAir)
            {
                return;
            }

            if (dashInput.GetKeyDown())
            {
                if (Time.time - dashTime < cooldown)
                {
                    return;
                }

                Vector3 moveDirection = controller.GetMovementDirectionWorld();
                Vector3 lookDirection = controller.GetCharacterOrigin().forward;
                Vector3 velocity = moveDirection.sqrMagnitude > 0.01f ? moveDirection * force : lookDirection * force;

                if (isAdditive)
                {
                    controller.AddVelocity(velocity);
                }
                else
                {
                    controller.SetVelocity(velocity);
                }

                dashTime = Time.time;                
                OnStart?.Invoke();

                if (!controller.CollisionGround)
                {
                    dashCount++;
                }

                if (disableFriction)
                {
                    if (frictionTask != null)
                    {
                        frictionTask.Stop();
                        frictionTask = null;
                    }

                    frictionTask = this.WaitSecondsExt(0.25f, overrideFriction, resetFriction);
                }

                if (disableGravity)
                {
                    if (gravityTask != null)
                    {
                        gravityTask.Stop();
                        gravityTask = null;
                    }

                    gravityTask = this.WaitSecondsExt(0.25f, overrideGravity, resetGravity);
                }
            }
        }
        public void OnBeforeLook() { }

        private void OverrideFriction()
        {
            if (isFrictionOverriden)
            {
                return;
            }

            dashFriction = controller.GetGroundFriction();
            controller.SetGroundFriction(controller.GetGroundFriction() - dashFriction);

            isFrictionOverriden = true;
        }
        private void OverrideGravity()
        {
            if (isGravityOverriden)
            {
                return;
            }

            dashGravity = controller.GetGravity();
            controller.SetGravity(controller.GetGravity() - dashGravity);

            isGravityOverriden = true;
        }
        private void ResetFriction()
        {
            controller.SetGroundFriction(controller.GetGroundFriction() + dashFriction);

            isFrictionOverriden = false;
        }
        private void ResetGravity()
        {
            controller.SetGravity(controller.GetGravity() + dashGravity);

            isGravityOverriden = false;
        }

        public bool GetIsEnabled() => isEnabled.IsEnabled;
        public void Disable(out int token) => isEnabled.Disable(out token);
        public void Enable(ref int token) => isEnabled.Enable(ref token);
    }
}
