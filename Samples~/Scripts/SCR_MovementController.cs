using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using Core.Editor;
#endif

using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;
    using static InputDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        public event Action<MovementContext> OnStateChanged = null;

        public bool IsMoving => Move.GetAxis().x != 0 || Move.GetAxis().y != 0;
        public bool IsCrouching => movementCurrentStance == MovementStance.CROUCH;
        public bool IsWalking => isWalking;
        public bool IsSprinting => isSprinting;
        public bool CollisionGround => collisionGround;
        public bool CollisionCeiling => collisionCeiling;
        public bool CollisionSides => collisionSides;
        public bool IsOnSteepSlope => isOnSteepSlope;
        public bool IsOnWalkableSlope => isOnWalkableSlope;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = 0;
        [SerializeField] private bool collisionGizmosCeiling = false;
        [SerializeField] private float collisionCeilingRadiusOffset = 0f;
        [SerializeField] private float collisionCeilingOffset = 0f;
        [SerializeField] private bool collisionGizmosGround = false;
        [SerializeField] private float collisionGroundRadiusOffset = 0f;
        [SerializeField] private float collisionGroundOffset = 0.075f;
        [SerializeField] private bool4 collisionGizmosSides = new(false, false, false, false);
        [SerializeField] private float collisionSidesRadiusOffset = 0f;
        [SerializeField] private float collisionSidesOffset = 0.05f;

        [Header("_")]
        [SerializeField, Min(1)] private float stanceStandTransitionRoughness = 5;
        [SerializeField, Min(1)] private float stanceCrouchTransitionRoughness = 5;
        [SerializeField, Range(0, 2)] private float stanceStandColliderHeight = 2;
        [SerializeField, Range(0, 2)] private float stanceCrouchColliderHeight = 1.35f;
        [SerializeField, Min(0)] private float stanceStandColliderRadius = 0.5f;
        [SerializeField, Min(0)] private float stanceCrouchColliderRadius = 0.5f;
        [SerializeField] private float stanceStandCameraHeight = 1.666f;
        [SerializeField] private float stanceCrouchCameraHeight = 1.000f;

        [Header("_")]
        [SerializeField, Required] private Transform cameraOrigin = null;
        [SerializeField, Range(45, 90)] private float cameraFieldOfView = 60.0f;
        [SerializeField, Min(0)] private float cameraSensitivity = 1.25f;
        [SerializeField, Range(0, 90)] private float cameraPitchClampAngle = 75;
        [SerializeField, Range(0, 90)] private float cameraYawClampAngle = 0;
        [SerializeField, ReadOnly] private float cameraYawClampCenter = 0;

        [Header("_")]
        [SerializeField, Min(0)] private float movementGravity = 32.0f;
        [SerializeField, Min(0)] private float movementJumpCoyote = 0.5f;
        [SerializeField, Min(0)] private float movementJumpForce = 10.0f;
        [SerializeField, Min(0)] private float movementCrouchSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementCrouchSpeedTransition = 1.5f;
        [SerializeField, Min(0)] private float movementSprintSpeedMultiplier = 2.0f;
        [SerializeField, Min(0)] private float movementSprintSpeedTransition = 1.5f;
        [SerializeField, Min(0)] private float movementWalkSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementWalkSpeedTransition = 1.5f;
        [SerializeField, Min(0)] private float movementBackwardSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementBackwardSpeedTransition = 1.5f;
        [SerializeField, Min(0)] private float movementGroundSpeed = 2.5f;
        [SerializeField, Min(0)] private float movementGroundAccelerate = 6.0f;
        [SerializeField, Min(0)] private float movementGroundFriction = 5.0f;
        [SerializeField, Min(0)] private float movementAirSpeed = 1.15f;
        [SerializeField, Min(0)] private float movementAirAccelerate = 8192f;

        [Header("_")]
        [SerializeField] private bool autoSprint = false;
        [SerializeField] private bool toggleSprint = false;
        [SerializeField] private bool toggleCrouch = false;
        [SerializeField] private bool toggleWalk = false;

        [Header("_")]
        [SerializeField] private LayerMask stepMask = 0;
        [SerializeField, Min(0f)] private float stepIntervalMin = 0.3125f;
        [SerializeField, Min(0f)] private float stepIntervalMax = 0.675f;

        private CharacterController characterController = null;
        private Transform characterOrigin = null;
        private Camera cameraController = null;
        private Collider[] characterColliders = null;
        private Collider lastCollidedCollider = null;
        private readonly HashSet<Collider> collisionCurrentColliders = new();
        private readonly HashSet<Collider> collisionLastColliders = new();
        private readonly StackBool isMovementEnabled = new();
        private readonly StackBool isSprintEnabled = new();
        private readonly StackBool isWalkEnabled = new();
        private readonly StackBool isCrouchEnabled = new();
        private readonly StackBool isJumpEnabled = new();
        private readonly StackBool isLookEnabled = new();
        private readonly StackBool isGravityEnabled = new();
        private readonly StackBool isCollidersEnabled = new();
        private Vector3 movementDirection = Vector3.zero;
        private Vector3 movementVelocity = Vector3.zero;
        private Vector3 stanceOffset = Vector3.zero;
        private Vector3 cameraPosition = Vector3.zero;
        private Quaternion cameraRotation = Quaternion.identity;
        private LayerMask collisionDefaultMask = -1;
        private RaycastHit collisionStepInfo = new();
        private RaycastHit collisionGroundInfo = new();
        private RaycastHit collisionSidesInfo = new();
        private RaycastHit collisionCeilingInfo = new();
        private Vector3 collisionCeilingDirection = Vector3.zero;
        private Vector3 collisionCeilingPosition = Vector3.zero;
        private Vector3 collisionGroundDirection = Vector3.zero;
        private Vector3 collisionGroundPosition = Vector3.zero;
        private Vector3 collisionSideTopPosition = Vector3.zero;
        private Vector3 collisionSideBottomPosition = Vector3.zero;
        private MovementStance movementCurrentStance = MovementStance.STAND;
        private float collisionGroundRadius = 0;
        private float collisionCeilingRadius = 0;
        private float collisionSidesRadius = 0;
        private float collisionAngle = 0;
        private float cameraPitchRaw = 0;
        private float cameraYawRaw = 0;
        private float cameraRollRaw = 0;
        private float cameraPitch = 0;
        private float cameraYaw = 0;
        private float cameraRoll = 0;
        private float lookWeight = 1;
        private float movementTargetSpeed = 1;
        private float movementCurrentSpeed = 1;
        private float movementNormalizedSpeed = 1;
        private float groundTimer = 0;
        private float fallTimer = 0;
        private float fallHeight = 0;
        private float fallPosition = 0;
        private float stepTimer = 0;
        private float stepInterval = 0;
        private bool collisionGround = false;
        private bool collisionCeiling = false;
        private bool collisionSides = false;
        private bool wasJumped = false;
        private bool wasGrounded = false;
        private bool wishJump = false;
        private bool wishCrouch = false;
        private bool wishSprint = false;
        private bool wishWalk = false;
        private bool canJump = false;
        private bool canCrouch = false;
        private bool canStand = false;
        private bool isSprinting = false;
        private bool isWalking = false;
        private bool isStanceOverrided = false;
        private bool isCameraYawLocked = false;
        private bool isCameraPitchLocked = false;
        private bool isCameraRollLocked = false;
        private bool isFallHeightResolved = false;
        private bool isCollisionClipResolved = false;
        private bool isOnSteepSlope = false;
        private bool isOnWalkableSlope = false;

        private void Awake()
        {
            collisionDefaultMask = collisionMask;

            cameraController = GetComponentInChildren<Camera>();
            characterOrigin = GetComponent<Transform>();
            characterController = GetComponent<CharacterController>();
            characterColliders = GetComponentsInChildren<Collider>();

            for (int i = 0; i < characterColliders.Length; i++)
            {
                characterColliders[i].includeLayers = collisionDefaultMask;
                characterColliders[i].excludeLayers = ~collisionDefaultMask;
            }
        }
        private void Start()
        {
            characterController.minMoveDistance = 0f;
            characterController.radius = stanceStandColliderRadius;
            characterController.height = stanceStandColliderHeight;
            characterController.center = (characterController.height / 2) * Vector3.up;
            characterController.includeLayers = collisionDefaultMask;
            characterController.excludeLayers = ~collisionDefaultMask;
            characterController.skinWidth = characterController.radius * 0.1f;
            stanceOffset = Vector3.up * stanceStandCameraHeight;
            cameraController.fieldOfView = cameraFieldOfView;
            SetCameraPitchClamp(cameraPitchClampAngle);
            SetCameraYawClamp(cameraYawClampAngle);
            this.WaitFrame(() => characterController.enabled = true);
        }
        private void Update()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            UpdateCollision();
            UpdateMovement();

            characterController.Move(movementVelocity * Time.deltaTime);
        }
        private void LateUpdate()
        {
            UpdateCamera();

            characterOrigin.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
            cameraOrigin.SetLocalPositionAndRotation(cameraPosition, cameraRotation);
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (characterController == null)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            if (collisionGizmosCeiling)
            {
                Gizmos.color = CollisionCeiling ? Color.red : Color.green;
                Gizmos.DrawRay(collisionCeilingPosition, collisionCeilingDirection);
                Gizmos.DrawWireSphere(collisionCeilingPosition + collisionCeilingDirection, collisionCeilingRadius);
            }

            if (collisionGizmosGround)
            {
                Gizmos.color = CollisionGround ? Color.red : Color.green;
                Gizmos.DrawRay(collisionGroundPosition, collisionGroundDirection);
                Gizmos.DrawWireSphere(collisionGroundPosition + collisionGroundDirection, collisionGroundRadius);
            }

            Gizmos.matrix = Matrix4x4.identity;
            float offset = characterController.skinWidth + collisionSidesOffset;
            Vector3 normal = collisionSidesInfo.normal;
            float isRight = Vector3.Dot(normal, characterOrigin.right);
            float isForward = Vector3.Dot(normal, characterOrigin.forward);

            if (collisionGizmosSides.x)
            {
                Gizmos.color = isForward < -0.5f ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, transform.forward, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.y)
            {
                Gizmos.color = isForward > 0.5f ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, -transform.forward, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.z)
            {
                Gizmos.color = isRight > 0.5f ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, transform.right, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.w)
            {
                Gizmos.color = isRight < 0.5f ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, -transform.right, collisionSidesRadius, offset);
            }
        }
        private void OnValidate()
        {
            if (characterController != null)
            {
                characterController.minMoveDistance = 0f;
                characterController.radius = stanceStandColliderRadius;
                characterController.height = stanceStandColliderHeight;
                characterController.center = (characterController.height / 2) * Vector3.up;
                characterController.includeLayers = collisionDefaultMask;
                characterController.excludeLayers = ~collisionDefaultMask;
                characterController.skinWidth = characterController.radius * 0.1f;
            }
            else
            {
                characterController = GetComponent<CharacterController>();
            }

            if (characterOrigin == null)
            {
                characterOrigin = GetComponent<Transform>();
            }
        }
#endif
        private void UpdateCollision()
        {
            collisionCurrentColliders.Clear();
            isCollisionClipResolved = false;

            characterController.height = movementCurrentStance == MovementStance.CROUCH ? stanceCrouchColliderHeight : stanceStandColliderHeight;
            characterController.radius = movementCurrentStance == MovementStance.CROUCH ? stanceCrouchColliderRadius : stanceStandColliderRadius;
            characterController.center = movementCurrentStance == MovementStance.CROUCH ? ((stanceCrouchColliderHeight / 2) * Vector3.up) : ((stanceStandColliderHeight / 2) * Vector3.up);

            RaycastHit bestHit = new();

            collisionGround = CalculateGroundCollision();
            if (collisionGround && collisionGroundInfo.collider != null)
            {
                collisionCurrentColliders.Add(collisionGroundInfo.collider);
                bestHit = collisionGroundInfo;
            }

            collisionCeiling = CalculateCeilingCollision();
            if (collisionCeiling && collisionCeilingInfo.collider != null)
            {
                collisionCurrentColliders.Add(collisionCeilingInfo.collider);
                bestHit = collisionGroundInfo;
            }

            collisionSides = CalculateSidesCollision();
            if (collisionSides && collisionSidesInfo.collider != null)
            {
                collisionCurrentColliders.Add(collisionSidesInfo.collider);
                bestHit = collisionGroundInfo;
            }

            collisionAngle = Vector3.Angle(collisionGroundInfo.normal, Vector3.up);
            isOnWalkableSlope = collisionAngle < characterController.slopeLimit;
            isOnSteepSlope = collisionAngle >= characterController.slopeLimit;

            foreach (Collider collider in collisionCurrentColliders)
            {
                if (!collisionLastColliders.Contains(collider))
                {
                    lastCollidedCollider = collider;
                    SetState(MovementState.COLLIDER_ENTER);
                }
            }

            foreach (Collider collider in collisionLastColliders)
            {
                if (!collisionCurrentColliders.Contains(collider))
                {
                    SetState(MovementState.COLLIDER_EXIT);
                }
            }

            collisionLastColliders.Clear();
            foreach (Collider collider in collisionCurrentColliders)
            {
                collisionLastColliders.Add(collider);
            }
        }
        private void UpdateMovement()
        {
            Vector2 input = GetIsMovementEnabled() ? Move.GetAxis() : Vector2.zero;

            movementDirection = characterOrigin.TransformVector(new(input.x, 0f, input.y));
            movementDirection = movementDirection.normalized;

            wishCrouch = toggleCrouch ? Crouch.GetKeyDown() ? wishCrouch = !wishCrouch : wishCrouch : Crouch.GetKey();
            wishSprint = toggleSprint ? Sprint.GetKeyDown() ? wishSprint = !wishSprint : wishSprint : autoSprint || Sprint.GetKey();
            wishWalk = toggleWalk ? Walk.GetKeyDown() ? wishWalk = !wishWalk : wishWalk : Walk.GetKey();
            wishJump = Jump.GetKeyDown();

            canJump =
                GetIsJumpEnabled() &&
                wishJump &&
                !wasJumped &&
                isOnWalkableSlope &&
                !CollisionCeiling &&
                (CollisionGround || fallTimer <= movementJumpCoyote);

            canCrouch =
                GetIsCrouchEnabled() &&
                movementCurrentStance == MovementStance.STAND &&
                wishCrouch &&
                !isStanceOverrided;

            canStand =
                movementCurrentStance == MovementStance.CROUCH &&
                !wishCrouch &&
                !isStanceOverrided &&
                !CollisionCeiling;

            isSprinting =
                GetIsSprintEnabled() &&
                movementCurrentStance == MovementStance.STAND &&
                wishSprint &&
                IsMoving &&
                Move.GetAxis().y > 0;

            isWalking =
                GetIsWalkEnabled() &&
                movementCurrentStance == MovementStance.STAND &&
                wishWalk &&
                !isSprinting &&
                !canCrouch &&
                !canJump;

            if (canCrouch)
            {
                if (movementCurrentStance != MovementStance.CROUCH)
                {
                    SetState(MovementState.CROUCH);
                }

                movementCurrentStance = MovementStance.CROUCH;
            }
            else if (canStand)
            {
                if (movementCurrentStance == MovementStance.CROUCH)
                {
                    SetState(MovementState.STAND);
                }

                movementCurrentStance = MovementStance.STAND;
            }

            if (CollisionGround)
            {
                wasJumped = false;

                if (!wasGrounded)
                {
                    fallHeight = fallPosition - characterOrigin.position.y;

                    SetState(MovementState.LAND);

                    if (movementVelocity.y < 0f && collisionAngle <= 5f)
                    {
                        movementVelocity.y = -1f;
                    }

                    TryStep();

                    wasGrounded = true;
                    isFallHeightResolved = false;
                }

                groundTimer += Time.deltaTime;
                fallTimer = 0;

                if (isOnSteepSlope)
                {
                    MoveSurf();
                }
                else
                {
                    MoveGround();
                }
            }
            else
            {
                if (wasGrounded)
                {
                    isFallHeightResolved = false;
                    wasGrounded = false;
                }

                if (CollisionCeiling && wasJumped)
                {
                    movementVelocity.y = -1;
                    wasJumped = false;
                }

                if (CollisionCeiling && movementVelocity.y > 0f)
                {
                    movementVelocity.y = -1f;
                }

                if (movementVelocity.y <= 0)
                {
                    if (!isFallHeightResolved)
                    {
                        fallPosition = characterOrigin.position.y;
                        isFallHeightResolved = true;
                    }

                    fallTimer += Time.deltaTime;
                }

                groundTimer = 0;
                MoveAir();
            }

            if (canJump)
            {
                MoveJump();
            }
        }
        private void UpdateCamera()
        {
            if (GetIsLookEnabled())
            {
                cameraPitchRaw -= Look.GetAxis().y * GetSensitivity() * lookWeight * 0.1f;
                cameraYawRaw += Look.GetAxis().x * GetSensitivity() * lookWeight * 0.1f;

                cameraPitchRaw = Mathf.Clamp(cameraPitchRaw, -cameraPitchClampAngle, cameraPitchClampAngle);

                if (cameraYawClampAngle > 0f)
                {
                    cameraYawRaw = Mathf.Clamp(cameraYawRaw, cameraYawClampCenter - cameraYawClampAngle, cameraYawClampCenter + cameraYawClampAngle);
                }
            }

            Vector3 stanceCenter = Vector3.up * (movementCurrentStance == MovementStance.CROUCH ? stanceCrouchCameraHeight : stanceStandCameraHeight);
            float stanceTime = (movementCurrentStance == MovementStance.CROUCH ? stanceCrouchTransitionRoughness : stanceStandTransitionRoughness);
            stanceOffset = Vector3.Lerp(stanceOffset, stanceCenter, Time.deltaTime * stanceTime);

            cameraPitch = isCameraPitchLocked ? cameraPitch : cameraPitchRaw;
            cameraYaw = isCameraYawLocked ? cameraYaw : cameraYawRaw;
            cameraRoll = isCameraRollLocked ? cameraRoll : cameraRollRaw;

            cameraRotation = Quaternion.Euler(cameraPitch, 0, cameraRoll);
            cameraPosition = stanceOffset;
        }

        private void MoveAir()
        {
            movementVelocity.y -= GetIsGravityEnabled() ? movementGravity * Time.deltaTime : 0;

            if (CollisionSides && !isCollisionClipResolved)
            {
                ApplyClipVelocity(collisionSidesInfo.normal);
                isCollisionClipResolved = true;
            }

            ApplyAirAcceleration(movementAirAccelerate, movementAirSpeed);
        }
        private void MoveSurf()
        {
            movementVelocity.y -= movementGravity * Time.deltaTime;

            if (!isCollisionClipResolved)
            {
                ApplyClipVelocity(collisionGroundInfo.normal);
                isCollisionClipResolved = true;
            }

            ApplyAirAcceleration(movementAirAccelerate, movementAirSpeed * 1.25f);
        }
        private void MoveGround()
        {
            bool movingBackwards = Move.GetAxis().y < 0;

            movementTargetSpeed = movementGroundSpeed;
            movementTargetSpeed *= IsSprinting ? movementSprintSpeedMultiplier : 1;
            movementTargetSpeed *= IsCrouching ? movementCrouchSpeedMultiplier : 1;
            movementTargetSpeed *= IsWalking ? movementWalkSpeedMultiplier : 1;
            movementTargetSpeed *= movingBackwards ? movementBackwardSpeedMultiplier : 1;

            float t = 1f;
            t = IsSprinting ? movementSprintSpeedTransition : t;
            t = IsCrouching ? movementCrouchSpeedTransition : t;
            t = IsWalking ? movementWalkSpeedTransition : t;
            t = movingBackwards ? movementBackwardSpeedTransition : t;

            movementCurrentSpeed = Mathf.Lerp(movementCurrentSpeed, movementTargetSpeed, t * Time.deltaTime);

            float maxPossibleSpeed = movementGroundSpeed * movementSprintSpeedMultiplier;
            movementNormalizedSpeed = Mathf.Clamp(movementTargetSpeed, 0, maxPossibleSpeed) / maxPossibleSpeed;

            movementDirection = Vector3.ProjectOnPlane(movementDirection, collisionGroundInfo.normal).normalized;

            ApplyFriction(wishJump || groundTimer < 0.0333f ? 0 : movementGroundFriction);
            if (IsMoving) ApplyGroundAcceleration(movementGroundAccelerate, movementCurrentSpeed);

            if (!CollisionSides)
            {
                movementVelocity = Vector3.ProjectOnPlane(movementVelocity, collisionGroundInfo.normal);
            }
            else if (CollisionSides && !isCollisionClipResolved)
            {
                ApplyClipVelocity(collisionSidesInfo.normal);
                isCollisionClipResolved = true;
            }

            stepInterval = Mathf.Lerp(stepIntervalMax, stepIntervalMin, movementNormalizedSpeed);
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0 && Time.timeScale != 0 && IsMoving)
            {
                TryStep();
                stepTimer = stepInterval;
            }
        }
        private void MoveJump()
        {
            movementVelocity.y = Mathf.Max(movementVelocity.y + movementJumpForce, movementJumpForce);

            SetState(MovementState.JUMP);
        }
        private void TryStep()
        {
            if (!GetIsMovementEnabled())
            {
                return;
            }

            if (Physics.Raycast(characterOrigin.position, Vector3.down, out collisionStepInfo, (GetCharacterHeight() / 2) + 0.125f, stepMask, QueryTriggerInteraction.Collide))
            {
                SetState(MovementState.STEP);
            }
        }

        public void SetState(MovementState state)
        {
            if (state == MovementState.JUMP)
            {
                wasJumped = true;
            }

            OnStateChanged?.Invoke(new(state, lastCollidedCollider, movementVelocity, collisionStepInfo, fallTimer, Mathf.Abs(movementVelocity.y), fallHeight));
        }

        private void ApplyClipVelocity(Vector3 normal, float overbounce = 1.05f) => movementVelocity = CalculateClipVelocity(movementVelocity, normal, overbounce);
        public Vector3 CalculateClipVelocity(Vector3 velocity, Vector3 normal, float overbounce = 1.05f)
        {
            float backoff = Vector3.Dot(velocity, normal);

            if (backoff >= 0)
            {
                return velocity;
            }

            backoff *= overbounce;

            velocity -= normal * backoff;

            return velocity;
        }

        private void ApplyGroundAcceleration(float factor, float maxSpeed) => movementVelocity = CalculateGroundAcceleration(movementVelocity, movementDirection, factor, maxSpeed);
        public Vector3 CalculateGroundAcceleration(Vector3 velocity, Vector3 direction, float factor, float maxSpeed)
        {
            float currentSpeed = Vector3.Dot(velocity, direction);
            float addSpeed = maxSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return velocity;
            }

            float accelerationSpeed = factor * Time.deltaTime * maxSpeed;
            accelerationSpeed = Mathf.Min(accelerationSpeed, addSpeed);

            velocity += direction * accelerationSpeed;

            return velocity;
        }

        private void ApplyAirAcceleration(float factor, float maxSpeed) => movementVelocity = CalculateAirAcceleration(movementVelocity, movementDirection, factor, maxSpeed);
        public Vector3 CalculateAirAcceleration(Vector3 velocity, Vector3 direction, float factor, float maxSpeed)
        {
            float currentSpeed = Vector3.Dot(velocity, direction);
            float addSpeed = maxSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return velocity;
            }

            float accelerationSpeed = factor * maxSpeed * Time.deltaTime;

            if (accelerationSpeed > addSpeed)
            {
                accelerationSpeed = addSpeed;
            }

            velocity += direction * accelerationSpeed;

            return velocity;
        }

        private void ApplyFriction(float factor) => movementVelocity = CalculateFriction(movementVelocity, factor);
        public Vector3 CalculateFriction(Vector3 velocity, float factor)
        {
            Vector3 horizontalVelocity = new(velocity.x, 0f, velocity.z);

            float currentSpeed = horizontalVelocity.magnitude;
            float frictionSpeed = factor * currentSpeed * Time.deltaTime;
            float finalSpeed = Mathf.Max(currentSpeed - frictionSpeed, 0f);

            if (currentSpeed > 0f)
            {
                finalSpeed /= currentSpeed;
                horizontalVelocity *= finalSpeed;

                velocity.x = horizontalVelocity.x;
                velocity.z = horizontalVelocity.z;
            }

            return velocity;
        }

        private bool CalculateSidesCollision()
        {
            collisionSidesRadius = characterController.radius + collisionSidesRadiusOffset;
            Vector3 origin = characterOrigin.position + ((characterController.height / 2) * Vector3.up);
            float height = (characterController.height / 2) - (collisionSidesRadius - characterController.skinWidth - 0.01f);
            float distance = characterController.skinWidth + collisionSidesOffset;

            collisionSideTopPosition = origin - Vector3.up * height;
            collisionSideBottomPosition = origin - Vector3.down * height;

            bool hit = false;
            float closestDistance = float.MaxValue;

            Vector3[] directions =
            {
                characterOrigin.forward,
                -characterOrigin.forward,
                characterOrigin.right,
                -characterOrigin.right
            };

            for (int i = 0; i < directions.Length; i++)
            {
                if (Physics.CapsuleCast(collisionSideTopPosition, collisionSideBottomPosition, collisionSidesRadius, directions[i], out RaycastHit hitInfo, distance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Angle(hitInfo.normal, Vector3.up) < characterController.slopeLimit)
                    {
                        continue;
                    }

                    if (hitInfo.distance < closestDistance)
                    {
                        closestDistance = hitInfo.distance;
                        collisionSidesInfo = hitInfo;

                        hit = true;
                    }
                }
            }

            return hit;
        }
        private bool CalculateGroundCollision()
        {
            collisionGroundRadius = characterController.radius + collisionGroundRadiusOffset;

            Vector3 rayOrigin = characterOrigin.position + ((characterController.height / 2) * Vector3.up);
            float rayHeight = (characterController.height / 2) - (collisionGroundRadius - characterController.skinWidth - 0.01f);

            collisionGroundDirection = Vector3.down * (rayHeight + collisionGroundOffset);
            collisionGroundPosition = rayOrigin;

            bool isHit = Physics.SphereCast(collisionGroundPosition, collisionGroundRadius, Vector3.down, out _, -collisionGroundDirection.y, collisionMask, QueryTriggerInteraction.Ignore);

            Physics.SphereCast(collisionGroundPosition, collisionGroundRadius, Vector3.down, out collisionGroundInfo, -collisionGroundDirection.y * 2.5f, collisionMask, QueryTriggerInteraction.Ignore);
            return isHit;
        }
        private bool CalculateCeilingCollision()
        {
            collisionCeilingRadius = characterController.radius + collisionCeilingRadiusOffset;

            Vector3 rayOrigin = characterOrigin.position + ((characterController.height / 2) * Vector3.up);
            float rayHeight = (characterController.height / 2) - (collisionCeilingRadius - characterController.skinWidth - 0.01f);

            collisionCeilingDirection = Vector3.up * (rayHeight + collisionCeilingOffset);
            collisionCeilingPosition = rayOrigin;

            return Physics.SphereCast(collisionCeilingPosition, collisionCeilingRadius, Vector3.up, out collisionCeilingInfo, collisionCeilingDirection.y, collisionMask, QueryTriggerInteraction.Ignore);
        }

        public void SyncCameraFromTransform()
        {
            Vector3 charEuler = characterOrigin.rotation.eulerAngles;
            cameraYaw = charEuler.y;

            Vector3 camEuler = cameraOrigin.localRotation.eulerAngles;

            float pitch = camEuler.x;
            if (pitch > 180f) pitch -= 360f; // Unity euler fix

            cameraPitch = pitch;
        }

        public void UnlockCameraPitch() => isCameraPitchLocked = false;
        public void LockCameraPitch() => isCameraPitchLocked = true;

        public void UnlockCameraYaw() => isCameraYawLocked = false;
        public void LockCameraYaw() => isCameraYawLocked = true;

        public void UnlockCameraRoll() => isCameraRollLocked = false;
        public void LockCameraRoll() => isCameraRollLocked = true;

        public float GetCameraRoll() => cameraRollRaw;
        public void SetCameraRoll(float value) => cameraRollRaw = value;

        public float GetCameraPitch() => cameraPitchRaw;
        public void SetCameraPitch(float value) => cameraPitchRaw = value;
        public void SetCameraPitchClamp(float value) => cameraPitchClampAngle = value;

        public float GetCameraYaw() => cameraYawRaw;
        public void SetCameraYaw(float value) => cameraYawRaw = value;
        public void SetCameraYawClamp(float value)
        {
            cameraYawClampAngle = Mathf.Max(0f, value);

            if (cameraYawClampAngle > 0f)
            {
                cameraYawClampCenter = cameraYawRaw;
            }
        }

        public Transform GetCameraOrigin() => cameraOrigin;
        public Transform GetCharacterOrigin() => characterOrigin;
        public float GetCharacterHeight() => characterController.height;
        public float GetCharacterWidth() => characterController.skinWidth;
        public float GetCharacterRadius() => characterController.radius;

        public MovementStance GetMovementStance() => movementCurrentStance;
        public void OverrideMovementStance(MovementStance value, bool @override)
        {
            movementCurrentStance = value;
            isStanceOverrided = @override;
        }

        public Vector3 GetMovementDirectionLocal() => characterOrigin.InverseTransformDirection(movementDirection);
        public Vector3 GetMovementDirectionWorld() => movementDirection;

        public RaycastHit GetGroundCollisionInfo() => collisionGroundInfo;
        public RaycastHit GetSidesCollisionInfo() => collisionSidesInfo;

        public LayerMask GetCollisionMask() => collisionMask;
        public void SetCollisionMask(LayerMask value)
        {
            collisionMask = value;

            for (int i = 0; i < characterColliders.Length; i++)
            {
                characterColliders[i].includeLayers = collisionMask;
                characterColliders[i].excludeLayers = ~collisionMask;
            }

            characterController.includeLayers = collisionMask;
            characterController.excludeLayers = ~collisionMask;
        }
        public void ResetCollisionMask() => SetCollisionMask(collisionDefaultMask);

        public void SetVelocity(Vector3 velocity) => movementVelocity = velocity;
        public void AddVelocity(Vector3 velocity) => movementVelocity += velocity;
        public Vector3 GetVelocity() => movementVelocity;

        public float GetSlopeLimit() => characterController.slopeLimit;
        public void SetSlopeLimit(float value) => characterController.slopeLimit = Mathf.Clamp(value, 0, 90);

        public float GetFieldOfView() => cameraFieldOfView;
        public void SetFieldOfView(float value)
        {
            cameraFieldOfView = value;

            cameraController.fieldOfView = cameraFieldOfView;
        }

        public float GetSensitivity() => cameraSensitivity;
        public void SetSensitivity(float value) => cameraSensitivity = value;

        public float GetLookWeight() => lookWeight;
        public void SetLookWeight(float value) => lookWeight = Mathf.Clamp01(value);

        public float GetGravity() => movementGravity;
        public void SetGravity(float value) => movementGravity = value;

        public float GetJumpForce() => movementJumpForce;
        public void SetJumpForce(float value) => movementJumpForce = value;

        public float GetSprintSpeedMult() => movementSprintSpeedMultiplier;
        public void SetSprintSpeedMult(float value) => movementSprintSpeedMultiplier = value;
        public float GetSprintSpeedTransition() => movementSprintSpeedTransition;
        public void SetSprintSpeedTransition(float value) => movementSprintSpeedTransition = value;

        public float GetCrouchSpeedMult() => movementCrouchSpeedMultiplier;
        public void SetCrouchSpeedMult(float value) => movementCrouchSpeedMultiplier = value;
        public float GetCrouchSpeedTransition() => movementCrouchSpeedTransition;
        public void SetCrouchSpeedTransition(float value) => movementCrouchSpeedTransition = value;

        public float GetWalkSpeedMult() => movementWalkSpeedMultiplier;
        public void SetWalkSpeedMult(float value) => movementWalkSpeedMultiplier = value;
        public float GetWalkSpeedTransition() => movementWalkSpeedTransition;
        public void SetWalkSpeedTransition(float value) => movementWalkSpeedTransition = value;

        public float GetBackwardSpeedMult() => movementBackwardSpeedMultiplier;
        public void SetBackwardSpeedMult(float value) => movementBackwardSpeedMultiplier = value;
        public float GetBackwardSpeedTransition() => movementBackwardSpeedTransition;
        public void SetBackwardSpeedTransition(float value) => movementBackwardSpeedTransition = value;

        public float GetNormalizedSpeed() => movementNormalizedSpeed;
        public float GetCurrentSpeed() => movementVelocity.ClearY().magnitude;

        public float GetGroundSpeed() => movementGroundSpeed;
        public void SetGroundSpeed(float value) => movementGroundSpeed = value;

        public float GetGroundFriction() => movementGroundFriction;
        public void SetGroundFriction(float value) => movementGroundFriction = value;

        public float GetGroundAcceleration() => movementGroundAccelerate;
        public void SetGroundAcceleration(float value) => movementGroundAccelerate = value;

        public float GetAirSpeed() => movementAirSpeed;
        public void SetAirSpeed(float value) => movementAirSpeed = value;

        public float GetAirAcceleration() => movementAirAccelerate;
        public void SetAirAcceleration(float value) => movementAirAccelerate = value;

        public void EnableCharacterController()
        {
            if (this.enabled)
            {
                return;
            }

            this.enabled = true;
            characterController.enabled = true;
            this.WaitFrame(() => SetVelocity(Vector3.zero));
        }
        public void DisableCharacterController()
        {
            if (!this.enabled)
            {
                return;
            }

            this.enabled = false;
            characterController.enabled = false;
            this.WaitFrame(() => SetVelocity(Vector3.zero));
        }

        public void EnableCameraRendering() => cameraController.enabled = true;
        public void DisableCameraRendering() => cameraController.enabled = false;

        public void DisableSprint(object obj) => isSprintEnabled.Disable(obj);
        public void EnableSprint(object obj) => isSprintEnabled.Enable(obj);
        public bool GetIsSprintEnabled() => GetIsMovementEnabled() && isSprintEnabled.IsEnabled && this.enabled;

        public void DisableWalk(object obj) => isWalkEnabled.Disable(obj);
        public void EnableWalk(object obj) => isWalkEnabled.Enable(obj);
        public bool GetIsWalkEnabled() => GetIsMovementEnabled() && isWalkEnabled.IsEnabled && this.enabled;

        public void DisableCrouch(object obj) => isCrouchEnabled.Disable(obj);
        public void EnableCrouch(object obj) => isCrouchEnabled.Enable(obj);
        public bool GetIsCrouchEnabled() => GetIsMovementEnabled() && isCrouchEnabled.IsEnabled && this.enabled;

        public void DisableJump(object obj) => isJumpEnabled.Disable(obj);
        public void EnableJump(object obj) => isJumpEnabled.Enable(obj);
        public bool GetIsJumpEnabled() => GetIsMovementEnabled() && isJumpEnabled.IsEnabled && this.enabled;

        public void DisableMovement(object obj) => isMovementEnabled.Disable(obj);
        public void EnableMovement(object obj) => isMovementEnabled.Enable(obj);
        public bool GetIsMovementEnabled() => isMovementEnabled.IsEnabled && this.enabled;

        public void DisableGravity(object obj) => isGravityEnabled.Disable(obj);
        public void EnableGravity(object obj) => isGravityEnabled.Enable(obj);
        public bool GetIsGravityEnabled() => isGravityEnabled.IsEnabled && this.enabled;

        public void DisableLook(object obj) => isLookEnabled.Disable(obj);
        public void EnableLook(object obj) => isLookEnabled.Enable(obj);
        public bool GetIsLookEnabled() => isLookEnabled.IsEnabled && this.enabled;

        public void DisableColliders(object obj) => isCollidersEnabled.Disable(obj);
        public void EnableColliders(object obj) => isCollidersEnabled.Enable(obj);
        public bool GetIsCollidersEnabled() => isCollidersEnabled.IsEnabled && this.enabled;
    }
}
