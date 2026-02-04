using System;
using UnityEngine;

namespace Core.Misc
{
    using static CoreUtility;

    public enum VaultType { SHORT = 0, STANDART = 1, SPEED = 2, CLIMB = 3 }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementProcessorVault : MonoBehaviour, IMovementProcessor
    {
        public event Action<VaultType> OnStart = null;
        public event Action<VaultType> OnEnd = null;

        public int Priority => 3;
        public bool IsVaulting => isVaulting;
        public VaultType VaultType => vaultType;
        public float VaultHeight => vaultHeight;

        [Header("_")]
        [SerializeField] private bool showGizmos = false;
        [SerializeField] private LayerMask layer = 0;
        [SerializeField, Min(0.1f)] private float minHeight = 0.5f;
        [SerializeField, Min(0.1f)] private float maxHeight = 2.25f;
        [SerializeField, Min(0.1f)] private float distance = 1.0f;

        [Header("_")]
        [SerializeField] private float duration = 0.425f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float maxSpeed = 12f;

        private MovementController movementController = null;
        private readonly StackBool isEnabled = new(8);
        private Vector3 enterVelocity = Vector3.zero;
        private Vector3 vaultVelocity = Vector3.zero;
        private VaultType vaultType = VaultType.SHORT;
        private float vaultHeight = 0f;
        private float vaultTimer = 0f;
        private readonly float vaultCooldown = 0.2f;
        private int movementToken = 0;
        private bool canVault = false;
        private bool isVaulting = false;

        private void Awake() => movementController = GetComponent<MovementController>();
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos)
            {
                return;
            }

            if (movementController == null)
            {
                return;
            }

            Vector3 rayOrigin = GetRayOrigin();
            float rayLength = GetRayLength();

            Gizmos.color = COLOR_YELLOW;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength);

            if (TryGetVaultTarget(out Vector3 vaultPosition, out _))
            {
                Gizmos.color = COLOR_GREEN;
                Gizmos.DrawSphere(vaultPosition, 0.1f);
            }
        }
#endif

        public void OnBeforeMove(MovementController controller)
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (isVaulting)
            {
                UpdateVault();
                return;
            }

            TryVault();
        }
        public void OnBeforeLook(MovementController controller) { }

        private bool TryGetVaultTarget(out Vector3 vaultPosition, out float vaultHeight)
        {
            vaultPosition = Vector3.zero;
            vaultHeight = 0;

            Vector3 rayOrigin = GetRayOrigin();
            float rayLength = GetRayLength();

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, rayLength, layer))
            {
                float vaultAngle = Vector3.Angle(hitInfo.normal, Vector3.up);

                if (vaultAngle > 5)
                {
                    return false;
                }

                vaultHeight = hitInfo.point.y - movementController.GetCharacterOrigin().position.y;

                if (vaultHeight >= minHeight && vaultHeight <= maxHeight)
                {
                    vaultPosition = hitInfo.point + Vector3.up * 0.1f;
                    return true;
                }
            }

            return false;
        }
        private void TryVault()
        {
            if (!GetIsEnabled())
            {
                return;
            }

            if (canVault)
            {
                return;
            }

            if (movementController.CollisionGround)
            {
                return;
            }

            if (movementController.CollisionCeiling)
            {
                return;
            }

            Vector3 horizontalVelocity = movementController.GetVelocity().ClearY();

            if (horizontalVelocity.magnitude < 0.1f)
            {
                return;
            }

            if (movementController.GetMovementDirectionLocal().z < 0)
            {
                return;
            }    

            if (TryGetVaultTarget(out Vector3 vaultPosition, out float vaultHeight))
            {
                StartVault(vaultPosition, vaultHeight);
            }
        }
        private void StartVault(Vector3 vaultPosition, float vaultHeight)
        {
            if (isVaulting)
            {
                return;
            }

            isVaulting = true;
            vaultTimer = 0f;

            enterVelocity = movementController.GetVelocity().ClearY();

            this.vaultHeight = vaultHeight;
            vaultType = CalculateVaultType(enterVelocity.magnitude, vaultHeight);
            vaultVelocity = CalculateVaultVelocity(movementController.GetCharacterOrigin().position, vaultPosition, duration);

            switch (vaultType)
            {
                case VaultType.SHORT:
                    break;
                case VaultType.STANDART:
                    enterVelocity = enterVelocity.Clamp(10f);
                    break;
                case VaultType.SPEED:
                    enterVelocity = enterVelocity.Clamp(10f);
                    break;
                case VaultType.CLIMB:
                    enterVelocity = enterVelocity.Clamp(2.5f);
                    break;
            }

            vaultVelocity += enterVelocity + movementController.GetCharacterOrigin().forward;

            movementController.DisableMovement(out movementToken);

            this.WaitSeconds(vaultCooldown, () => canVault = true, () => canVault = false);
            OnStart?.Invoke(vaultType);
        }
        private void UpdateVault()
        {
            vaultTimer += Time.deltaTime;

            vaultVelocity.y += gravity * Time.deltaTime;

            movementController.SetVelocity(vaultVelocity);

            if (vaultTimer >= duration)
            {
                EndVault();
            }
        }
        private void EndVault()
        {
            if (!isVaulting)
            {
                return;
            }

            isVaulting = false;

            movementController.SetVelocity(vaultVelocity);
            movementController.EnableMovement(ref movementToken);

            OnEnd?.Invoke(vaultType);
        }

        private float GetRayLength() => maxHeight + 0.5f;
        private Vector3 GetRayOrigin() => movementController.GetCameraOrigin().position + movementController.GetCharacterOrigin().forward * distance;
        private VaultType CalculateVaultType(float momentum, float height)
        {
            if (height > maxHeight * 0.9f) return VaultType.CLIMB;
            if (momentum > 6f) return VaultType.SPEED;
            if (momentum > 3f) return VaultType.STANDART;
            return VaultType.SHORT;
        }
        private Vector3 CalculateVaultVelocity(Vector3 start, Vector3 target, float time)
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
