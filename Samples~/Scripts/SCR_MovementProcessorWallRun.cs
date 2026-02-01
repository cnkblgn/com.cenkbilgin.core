using System;
using UnityEngine;
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(MovementController))]
    public class MovementProcessorWallRun : MonoBehaviour, IMovementProcessor
    {
        public event Action OnStart = null;
        public event Action OnEnd = null;
        public event Action<RaycastHit> OnStep = null;

        public int Priority => 2;
        public bool IsWallRunning => isWallRunning;
        public bool HasWallLeft => hasWallLeft;
        public bool HasWallRight => hasWallRight;

        [Header("_")]
        [SerializeField] private bool showGizmos = false;
        [SerializeField, Min(1)] private float minHeight = 2.5f;
        [SerializeField, Min(0.1f)] private float distance = 1.33f;

        [Header("_")]
        [SerializeField, Min(1)] private float speed = 8f;
        [SerializeField, Min(0.1f)] private float duration = 10f;
        [SerializeField, Min(1)] private float gravity = 5f;
        [SerializeField, Min(0)] private float ejectForwardWeight = 1f;
        [SerializeField, Min(0)] private float ejectNormalWeight = 0.25f;
        [SerializeField, Min(0)] private float ejectUpWeight = 0.75f;

        private MovementController movementController;
        private readonly StackBool isEnabled = new();
        private RaycastHit wallInfo = new();
        private Collider wallCollider = null;
        private Vector3 wallVelocity = Vector3.zero;
        private Vector3 wallNormal = Vector3.up;
        private Vector3 wallSmoothNormal = Vector3.up;
        private bool isWallRunning = false;
        private bool hasWallLeft = false;
        private bool hasWallRight = false;
        private float wallRunTimer = 0;
        private float wallLastTime = 0;
        private float wallStepTimer = 0;
        private float wallStepInterval = 0;

        private void Awake() => movementController = GetComponent<MovementController>();
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
            
            Transform origin = movementController.GetCharacterOrigin(); 
            Vector3 position = origin.position + 0.5f * movementController.GetCharacterHeight() * Vector3.up; 

            Gizmos.color = hasWallRight ? COLOR_GREEN : COLOR_RED; 
            Gizmos.DrawRay(position, origin.right * distance);
            Gizmos.color = hasWallLeft ? COLOR_GREEN : COLOR_RED;
            Gizmos.DrawRay(position, -origin.right * distance); 
        }

        public void OnBeforeMove(MovementController controller) => UpdateWallRun();
        public void OnBeforeLook(MovementController controller) { }

        private bool TryGetWallTarget(out RaycastHit hitInfo)
        {
            Transform origin = movementController.GetCharacterOrigin();
            Vector3 pos = origin.position + Vector3.up * (movementController.GetCharacterHeight() * 0.5f);
            hitInfo = new();

            hasWallRight = Physics.Raycast(pos, origin.right, out RaycastHit hitR, distance);
            hasWallLeft = Physics.Raycast(pos, -origin.right, out RaycastHit hitL, distance);

            // Eğer ikisi de true ise: HER ZAMAN right wall seçiliyor Sol duvara daha yakın olsan bile umursamıyor
            RaycastHit hit = hasWallRight ? hitR : hitL;

            if (!hasWallLeft && !hasWallRight)
            {
                return false;
            }

            if (Vector3.Angle(hit.normal, Vector3.up) < 85f)
            {
                return false;
            }

            if (Time.time - wallLastTime < 1.5f && wallCollider != null && hit.collider == wallCollider)
            {
                return false;
            }

            hitInfo = hit;

            return true;
        }

        private void UpdateWallRun()
        {
            if (movementController.CollisionGround)
            {
                EndWallRun();
                return;
            }

            if (Move.GetAxis().y < 0.5f)
            {
                EndWallRun();
                return;
            }

            if (!isWallRunning && !Jump.GetKey())
            {
                EndWallRun();
                return;
            }

            if (!TryGetWallTarget(out wallInfo))
            {
                EndWallRun();
                return;
            }

            wallNormal = wallInfo.normal;

            if (!isWallRunning)
            {
                StartWallRun();
                return;
            }

            wallRunTimer += Time.deltaTime;
            if (wallRunTimer > duration)
            {
                EndWallRun();
                return;
            }

            if (isWallRunning && Jump.GetKeyDown())
            {
                EjectWallRun();
                return;
            }

            wallSmoothNormal = Vector3.Slerp(wallSmoothNormal, wallNormal, 7.5f * Time.deltaTime);
            Transform origin = movementController.GetCharacterOrigin();
            Vector3 currentForward = wallVelocity.ClearY().normalized;
            Vector3 targetForward = Vector3.Cross(wallSmoothNormal, Vector3.up);

            if (Vector3.Dot(targetForward, origin.forward) < 0)
            {
                targetForward = -targetForward;
            }

            float cornerAngle = Vector3.Angle(currentForward, targetForward);

            if (cornerAngle > 60)
            {
                EndWallRun();
                return;
            }

            Vector3 forward = Vector3.Slerp(currentForward, targetForward, 7.5f * Time.deltaTime);
            Vector3 horizontal = wallVelocity.ClearY();
            horizontal = forward * Mathf.Max(horizontal.magnitude, speed);

            wallVelocity.x = horizontal.x;
            wallVelocity.z = horizontal.z;
            wallVelocity.y -= gravity * Time.deltaTime;

            movementController.SetVelocity(wallVelocity);

            wallStepInterval = Mathf.Lerp(0.666f, 0.333f, horizontal.magnitude / 10);
            wallStepTimer -= Time.deltaTime;
            if (wallStepTimer <= 0)
            {
                OnStep?.Invoke(wallInfo);
                wallStepTimer = wallStepInterval;
            }
        }
        private void StartWallRun()
        {
            if (!GetIsEnabled())
            {
                return;
            }

            if (isWallRunning)
            {
                return;
            }

            if (Physics.Raycast(movementController.GetCharacterOrigin().position + Vector3.up * (movementController.GetCharacterHeight() * 0.5f), Vector3.down, out RaycastHit hitG, minHeight))
            {
                return;
            }

            isWallRunning = true;
            wallRunTimer = 0f;

            wallSmoothNormal = wallNormal;
            wallVelocity = movementController.GetVelocity();
            wallVelocity.y = Mathf.Max(0, wallVelocity.y);

            movementController.SetIsMovementEnabled(false);
            OnStart?.Invoke();
        }
        private void EndWallRun()
        {
            if (!isWallRunning)
            {
                return;
            }

            isWallRunning = false;
            wallRunTimer = 0f;
            wallLastTime = Time.time;
            wallCollider = wallInfo.collider;

            movementController.SetIsMovementEnabled(true);
            movementController.SetVelocity(wallVelocity);

            OnEnd?.Invoke();
        }
        private void EjectWallRun()
        {
            EndWallRun();

            float magnitude = this.wallVelocity.magnitude;

            Vector3 velocity =
                magnitude * ejectForwardWeight * movementController.GetCharacterOrigin().forward.ClearY().normalized +
                magnitude * ejectUpWeight * Vector3.up +
                magnitude * ejectNormalWeight * wallSmoothNormal;

            movementController.SetVelocity(Vector3.ClampMagnitude(velocity, magnitude * 1.1f));
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
                EndWallRun();
                isEnabled.Disable();
            }
        }
    }
}