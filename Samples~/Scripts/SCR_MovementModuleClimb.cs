using System;
using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    public enum ClimbType { DEFAULT = 0, FAST = 1, CLIMB = 2 }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementModuleClimb : MonoBehaviour, IMovementModule
    {
        public event Action<ClimbType> OnStart = null;
        public event Action<ClimbType> OnEnd = null;

        public int Priority => 3;
        public bool IsClimbing => isClimbing;
        public ClimbType ClimbType => climbType;
        public float ClimbHeight => climbHeight;

        [Header("_")]
        [SerializeField] private bool showGizmos = false;
        [SerializeField] private LayerMask layer = 0;
        [SerializeField, Min(0.1f)] private float minHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float maxHeight = 1.5f;
        [SerializeField, Min(0.1f)] private float distance = 1.0f;

        [Header("_")]
        [SerializeField] private bool carryMomentum = true;
        [SerializeField] private bool disableLook = false;
        [SerializeField] private float duration = 0.425f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float maxSpeed = 12f;

        private MovementController controller = null;
        private readonly StackBool isEnabled = new(8);
        private Vector3 enterVelocity = Vector3.zero;
        private Vector3 climbVelocity = Vector3.zero;
        private RaycastHit climbInfo = new();
        private ClimbType climbType = ClimbType.DEFAULT;
        private float climbHeight = 0f;
        private float climbTimer = 0f;
        private readonly float climbCooldown = 0.2f;
        private int movementToken = 0;
        private int lookToken = 0;
        private bool canClimb = false;
        private bool isClimbing = false;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos)
            {
                return;
            }

            if (controller == null)
            {
                return;
            }

            Vector3 rayOrigin = GetRayOrigin();
            float rayLength = GetRayLength();

            Gizmos.color = COLOR_YELLOW;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);

            Gizmos.color = TryGetClimbTarget(out Vector3 climbPosition, out _) ? COLOR_GREEN : COLOR_RED;
            Gizmos.DrawSphere(climbPosition, 0.1f);
        }
#endif

        public void Bind(MovementController controller)
        {
            this.controller = controller;
        }
        public void Unbind(MovementController controller)
        {

        }

        public void OnBeforeMove()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (isClimbing)
            {
                UpdateClimb();
                return;
            }

            TryClimb();
        }
        public void OnBeforeLook() { }

        private bool TryGetClimbTarget(out Vector3 climbPosition, out float climbHeight)
        {
            climbPosition = Vector3.zero;
            climbHeight = 0;

            Vector3 rayOrigin = GetRayOrigin();
            float rayLength = GetRayLength();

            if (Physics.Raycast(rayOrigin, Vector3.down, out climbInfo, rayLength, layer))
            {
                float climbAngle = Vector3.Angle(climbInfo.normal, Vector3.up);
                climbHeight = climbInfo.point.y - controller.GetCharacterOrigin().position.y;
                climbPosition = climbInfo.point + Vector3.up * 0.1f;

                if (climbAngle > 5)
                {
                    return false;
                }

                if (climbHeight >= minHeight && climbHeight <= maxHeight)
                {                   
                    return true;
                }
            }

            return false;
        }
        private void TryClimb()
        {
            if (!GetIsEnabled())
            {
                return;
            }

            if (canClimb)
            {
                return;
            }

            if (controller.CollisionGround)
            {
                return;
            }

            if (controller.CollisionCeiling)
            {
                return;
            }

            Vector3 horizontalVelocity = controller.GetVelocity().ClearY();

            if (horizontalVelocity.magnitude < 0.1f)
            {
                return;
            }

            if (controller.GetMovementDirectionLocal().z < 0)
            {
                return;
            }    

            if (TryGetClimbTarget(out Vector3 climbPosition, out float climbHeight))
            {
                StartClimb(climbPosition, climbHeight);
            }
        }
        private void StartClimb(Vector3 climbPosition, float climbHeight)
        {
            if (isClimbing)
            {
                return;
            }

            isClimbing = true;
            climbTimer = 0f;

            enterVelocity = controller.GetVelocity().ClearY();

            this.climbHeight = climbHeight;
            this.climbType = CalculateClimbType(enterVelocity.magnitude, climbHeight);
            this.climbVelocity = CalculateClimbVelocity(controller.GetCharacterOrigin().position, climbPosition, duration);

            if (carryMomentum) climbVelocity += enterVelocity.Clamp(climbType == ClimbType.CLIMB ? 1 : enterVelocity.magnitude) + controller.GetCharacterOrigin().forward;

            controller.DisableMovement(out movementToken);
            if (disableLook) controller.DisableLook(out lookToken);

            this.WaitSeconds(climbCooldown, () => canClimb = true, () => canClimb = false);
            OnStart?.Invoke(climbType);
        }
        private void UpdateClimb()
        {
            climbTimer += Time.deltaTime;

            climbVelocity.y += gravity * Time.deltaTime;

            controller.SetVelocity(climbVelocity);

            if (climbTimer >= duration)
            {
                EndClimb();
            }
        }
        private void EndClimb()
        {
            if (!isClimbing)
            {
                return;
            }

            isClimbing = false;

            controller.SetVelocity(climbVelocity);
            controller.EnableMovement(ref movementToken);
            if (disableLook) controller.EnableLook(ref lookToken);

            OnEnd?.Invoke(climbType);
        }

        private float GetRayLength() => controller.GetCharacterHeight() + 0.5f;
        private Vector3 GetRayOrigin() => controller.GetCameraOrigin().position + controller.GetCharacterOrigin().forward * distance;
        private ClimbType CalculateClimbType(float momentum, float height)
        {
            if (height > maxHeight * 0.75f)
            {
                return ClimbType.CLIMB;
            }

            if (momentum > 10f)
            {
                return ClimbType.FAST;
            }

            return ClimbType.DEFAULT;
        }
        private Vector3 CalculateClimbVelocity(Vector3 start, Vector3 target, float time)
        {
            Vector3 d = target - start;
            Vector3 h = d.ClearY();
            Vector3 v = h / time;

            v.y = (d.y - 0.5f * gravity * time * time) / time;

            return Vector3.ClampMagnitude(v, maxSpeed);
        }

        public bool GetIsEnabled() => isEnabled.IsEnabled;
        public void Disable(out int token) => isEnabled.Disable(out token);
        public void Enable(ref int token) => isEnabled.Enable(ref token);
    }
}
