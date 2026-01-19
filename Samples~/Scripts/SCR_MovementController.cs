using System;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using Core.Editor;
#endif
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        public event Action<Collider> OnColliderEnter = null;
        public event Action<Collider> OnColliderExit = null;
        public event Action<float, float, float> OnLand = null; // fallTimer, fallVelocity.y, fallHeight
        public event Action OnJump = null;
        public event Action OnCrouch = null;
        public event Action OnStand = null;
        public event Action<RaycastHit> OnStep = null;

        public bool IsMoving => Move.GetAxis().x != 0 || Move.GetAxis().y != 0;
        public bool IsCrouching => movementCurrentStance == MovementStance.CROUCH;
        public bool IsWalking => canWalk;
        public bool IsSprinting => canSprint;
        public bool CollisionGround => collisionGround;
        public bool CollisionCeiling => collisionCeiling;
        public bool CollisionSides => collisionSides;

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
        [SerializeField, Range(0, 2)] private float stanceCrouchColliderHeight = 1;
        [SerializeField, Min(0)] private float stanceStandColliderRadius = 0.5f;
        [SerializeField, Min(0)] private float stanceCrouchColliderRadius = 0.5f;
        [SerializeField] private float stanceStandCameraHeight = 0.75f;
        [SerializeField] private float stanceCrouchCameraHeight = 0.35f;

        [Header("_")]
        [SerializeField, Required] private Transform cameraPivot = null;
        [SerializeField, Range(45, 90)] private float cameraFieldOfView = 60.0f;
        [SerializeField, Min(0)] private float cameraSensitivity = 1.25f;
        [SerializeField, Range(0, 90)] private float cameraPitchClampAngle = 75;
        [SerializeField, Range(0, 90)] private float cameraYawClampAngle = 0;
        [SerializeField, ReadOnly] private float cameraYawClampCenter = 0;
        [SerializeField, Range(0, 1), Tooltip("Strafe helper on air")] private float cameraStrafeControl = 0;

        [Header("_")]
        [SerializeField, Min(0)] private float movementGravity = 32.0f;
        [SerializeField, Min(0)] private float movementJumpCoyote = 0.5f;
        [SerializeField, Min(0)] private float movementJumpForce = 10.0f;
        [SerializeField, Min(0)] private float movementCrouchSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementSprintSpeedMultiplier = 2.0f;
        [SerializeField, Min(0)] private float movementWalkSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementBackwardsSpeedMultiplier = 0.5f;
        [SerializeField, Min(0)] private float movementGroundSpeed = 2.5f;
        [SerializeField, Min(0)] private float movementGroundAccelerate = 6.0f;
        [SerializeField, Min(1)] private float movementSpeedAccelerate = 1.5f;
        [SerializeField, Min(0)] private float movementGroundFriction = 5.0f;
        [SerializeField, Min(0)] private float movementAirSpeed = 1.15f;
        [SerializeField, Min(0)] private float movementAirAccelerate = 8192f;

        [Header("_")]
        [SerializeField] private LayerMask stepMask = 0;
        [SerializeField, Min(0f)] private float stepIntervalMin = 0.3125f;
        [SerializeField, Min(0f)] private float stepIntervalMax = 0.675f;

        private CharacterController characterController = null;
        private Transform characterOrigin = null;
        private Camera cameraController = null;
        private Collider collisionLastCollider = null;
        private Collider[] characterColliders = null;
        private readonly StackBool isInputEnabled = new();
        private readonly StackBool isMovementEnabled = new();
        private readonly StackBool isSprintEnabled = new();
        private readonly StackBool isCrouchEnabled = new();
        private readonly StackBool isJumpEnabled = new();
        private readonly StackBool isLookEnabled = new();
        private readonly StackBool isCollidersEnabled = new();
        private IMovementControllerProcessor[] movementProcessors = null;
        private Vector3 movementDirection = Vector3.zero;
        private Vector3 movementVelocity = Vector3.zero;
        private Vector3 cameraPosition = Vector3.zero;
        private Quaternion cameraRotation = Quaternion.identity;
        private readonly RaycastHit[] collisionSidesInfoBuffer = new RaycastHit[4];
        private LayerMask collisionDefaultMask = -1;
        private RaycastHit collisionGroundInfo = new();
        private RaycastHit collisionSidesInfo = new();
        private Vector3 collisionHeadDirection = Vector3.zero;
        private Vector3 collisionHeadPosition = Vector3.zero;
        private Vector3 collisionGroundDirection = Vector3.zero;
        private Vector3 collisionGroundPosition = Vector3.zero;
        private Vector3 collisionSideTopPosition = Vector3.zero;
        private Vector3 collisionSideBottomPosition = Vector3.zero;
        private MovementStance movementCurrentStance = MovementStance.STAND;
        private float collisionGroundRadius = 0;
        private float collisionHeadRadius = 0;
        private float collisionSidesRadius = 0;
        private float collisionAngle = 0;
        private float cameraRotationX = 0;
        private float cameraRotationY = 0;
        private float movementTargetGroundSpeed = 1;
        private float movementCurrentGroundSpeed = 1;
        private float movementNormalizedGroundSpeed = 1;
        private float groundTimer = 0;
        private float fallTimer = 0;
        private float fallHeight = 0;
        private float fallPosition = 0;
        private float stepTimer = 0;
        private float stepInterval = 0;
        private bool collisionGround = false;
        private bool collisionCeiling = false;
        private bool collisionSides = false;
        private bool wishJump = false;
        private bool wishCrouch = false;
        private bool wishSprint = false;
        private bool wishWalk = false;
        private bool canJump = false;
        private bool canCrouch = false;
        private bool canStand = false;
        private bool canSprint = false;
        private bool canWalk = false;
        private bool wasJumped = false;
        private bool wasGrounded = false;
        private bool wasVelocityAdded = false;
        private bool isJumpedLastFrame = false;
        private bool isNoclipActive = false;
        private bool isFallHeightResolved = false;
        private bool isClipResolved = false;
        private bool isLookOverrided = false;
        private bool isStanceOverrided = false;
        private bool isOnSteepSlope = false;
        private bool isOnWalkableSlope = false;

        private void Awake()
        {
            collisionDefaultMask = collisionMask;

            movementProcessors = GetComponents<IMovementControllerProcessor>();
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
            cameraPosition = Vector3.up * stanceStandCameraHeight;
            cameraController.fieldOfView = cameraFieldOfView;
            SetLookPitchClamp(cameraPitchClampAngle);
            SetLookYawClamp(cameraYawClampAngle);
            this.WaitFrame(null, () => characterController.enabled = true);
        }
        private void Update()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            UpdateCollision();
            UpdateMovement();

            foreach (IMovementControllerProcessor i in movementProcessors) i.OnAfterMoveTick();
        }
        private void LateUpdate()
        {
            UpdateCamera();

            foreach (IMovementControllerProcessor i in movementProcessors) i.OnAfterCameraTick();
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
                Gizmos.DrawRay(collisionHeadPosition, collisionHeadDirection);
                Gizmos.DrawWireSphere(collisionHeadPosition + collisionHeadDirection, collisionHeadRadius);
            }

            if (collisionGizmosGround)
            {
                Gizmos.color = CollisionGround ? Color.red : Color.green;
                Gizmos.DrawRay(collisionGroundPosition, collisionGroundDirection);
                Gizmos.DrawWireSphere(collisionGroundPosition + collisionGroundDirection, collisionGroundRadius);
            }

            Gizmos.matrix = Matrix4x4.identity;
            float offset = characterController.skinWidth + collisionSidesOffset;
            if (collisionGizmosSides.x)
            {
                Gizmos.color = collisionSidesInfoBuffer[0].collider != null ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, transform.forward, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.y)
            {
                Gizmos.color = collisionSidesInfoBuffer[1].collider != null ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, -transform.forward, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.z)
            {
                Gizmos.color = collisionSidesInfoBuffer[2].collider != null ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, transform.right, collisionSidesRadius, offset);
            }
            if (collisionGizmosSides.w)
            {
                Gizmos.color = collisionSidesInfoBuffer[3].collider != null ? Color.red : Color.green;
                EditorUtility.DrawCapsuleTarget(collisionSideTopPosition, collisionSideBottomPosition, -transform.right, collisionSidesRadius, offset);
            }
        }
        private void OnValidate()
        {
            if (cameraPivot != null)
            {
                cameraPivot.localPosition = Vector3.up * stanceStandCameraHeight;
            }

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
            /// NOT:
            /// Yüksek frekanslı yüzeylerde jitter oluyor. Buffer sistemi kullanarak bunu çözebiliriz.
            /// nedeni ise ground check range, biz angle i project ettiğimiz için bize ek olarak slope sphere cast info su lazım. normal ground dan daha uzun olacak ama isGrounded a karışmayacak yani

            isClipResolved = false;

            characterController.height = movementCurrentStance == MovementStance.CROUCH ? stanceCrouchColliderHeight : stanceStandColliderHeight;
            characterController.radius = movementCurrentStance == MovementStance.CROUCH ? stanceCrouchColliderRadius : stanceStandColliderRadius;
            characterController.center = movementCurrentStance == MovementStance.CROUCH ? ((stanceCrouchColliderHeight / 2) * Vector3.up) : ((stanceStandColliderHeight / 2) * Vector3.up);

            collisionGround = CalculateGroundCollision();
            collisionCeiling = CalculateHeadCollision();
            collisionSides = CalculateSidesCollision();
            collisionAngle = Vector3.Angle(collisionGroundInfo.normal, Vector3.up);
            isOnWalkableSlope = collisionAngle < characterController.slopeLimit;
            isOnSteepSlope = collisionAngle >= characterController.slopeLimit;

            Collider colliderThatHit = collisionGroundInfo.collider;

            if (collisionLastCollider != colliderThatHit && colliderThatHit != null)
            {
                OnColliderEnter?.Invoke(colliderThatHit);
                collisionLastCollider = colliderThatHit;
            }

            if (!CollisionGround && collisionLastCollider != null)
            {
                OnColliderExit?.Invoke(collisionLastCollider);
                collisionLastCollider = null;
            }
        }
        private void UpdateMovement()
        {
            if (!GetMovementEnabled())
            {
                return;
            }

            Vector2 input = GetIsInputEnabled() ? Move.GetAxis() : Vector2.zero;

            if (!CollisionGround && Mathf.Abs(input.x) < 0.01f)
            {
                input.x = Mathf.Lerp(input.x, Mathf.Clamp(Look.GetAxis().x * 5f, -1f, 1f), cameraStrafeControl);
            }

            movementDirection = characterOrigin.TransformVector(new(input.x, 0f, input.y));
            movementDirection = movementDirection.normalized;

            wishCrouch = Crouch.GetKey();
            wishSprint = Sprint.GetKey();
            wishJump = Jump.GetKey();

            canJump = GetJumpEnabled() && wishJump && !CollisionCeiling && isOnWalkableSlope && (!wasGrounded || groundTimer > 0f) && (CollisionGround || (!wasJumped && !isJumpedLastFrame && fallTimer <= movementJumpCoyote));
            canCrouch = !isStanceOverrided && GetCrouchEnabled() && wishCrouch && movementCurrentStance == MovementStance.STAND;
            canStand = !isStanceOverrided && !wishCrouch && movementCurrentStance == MovementStance.CROUCH && !CollisionCeiling;
            canSprint = GetSprintEnabled() && wishSprint && movementCurrentStance == MovementStance.STAND && IsMoving && Move.GetAxis().y > 0;
            canWalk = wishWalk && movementCurrentStance == MovementStance.STAND && !canSprint && !canCrouch && !canJump;

            if (isNoclipActive)
            {
                MoveNoclip();
                return; 
            }

            if (canCrouch)
            {
                if (movementCurrentStance != MovementStance.CROUCH)
                {
                    OnCrouch?.Invoke();
                }

                movementCurrentStance = MovementStance.CROUCH;
            }
            else if (canStand)
            {
                if (movementCurrentStance == MovementStance.CROUCH)
                {
                    OnStand?.Invoke();
                }

                movementCurrentStance = MovementStance.STAND;
            }

            if (CollisionGround)
            {
                if (!wasGrounded)
                {
                    fallHeight = fallPosition - characterOrigin.position.y;
                    OnLand?.Invoke(fallTimer, movementVelocity.y, fallHeight);

                    // LAND SNAP
                    if (movementVelocity.y < 0f && collisionAngle <= 5f)
                    {
                        movementVelocity.y = -1f;
                    }

                    TryStep();

                    wasGrounded = true;
                    isFallHeightResolved = false;
                }

                if (wasJumped)
                {
                    wasJumped = false;
                    isJumpedLastFrame = false; 
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

                if (CollisionCeiling && wasVelocityAdded)
                {
                    movementVelocity.y = -1;
                    wasVelocityAdded = false;
                }

                fallTimer += Time.deltaTime;
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

                wishJump = false;
                isJumpedLastFrame = true;

                if (!wasJumped)
                {
                    wasJumped = true;
                }
            }

            characterController.Move(movementVelocity * Time.deltaTime);
        }
        private void UpdateCamera()
        {
            if (isLookOverrided)
            {
                return;
            }

            Vector3 cameraCenter = Vector3.up * (movementCurrentStance == MovementStance.CROUCH ? stanceCrouchCameraHeight : stanceStandCameraHeight);

            float cameraTime = (movementCurrentStance == MovementStance.CROUCH ? stanceCrouchTransitionRoughness : stanceStandTransitionRoughness);

            cameraPosition = Vector3.Lerp(cameraPosition, cameraCenter, Time.deltaTime * cameraTime);
            cameraPivot.localPosition = cameraPosition;

            if (GetLookEnabled())
            {
                cameraRotationX -= Look.GetAxis().y * GetSensitivity() * 0.1f;
                cameraRotationY += Look.GetAxis().x * GetSensitivity() * 0.1f;
                cameraRotationX = Mathf.Clamp(cameraRotationX, -cameraPitchClampAngle, cameraPitchClampAngle);

                if (cameraYawClampAngle > 0f)
                {
                    cameraRotationY = Mathf.Clamp(cameraRotationY, cameraYawClampCenter - cameraYawClampAngle, cameraYawClampCenter + cameraYawClampAngle);
                }
            }

            characterOrigin.localRotation = Quaternion.Euler(0, cameraRotationY, 0);
            cameraPivot.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
        }

        private void MoveNoclip()
        {
            movementDirection = cameraPivot.TransformVector(new(Move.GetAxis().x, 0f, Move.GetAxis().y));
            canSprint = GetSprintEnabled() && wishSprint && movementCurrentStance == MovementStance.STAND && IsMoving;
            movementTargetGroundSpeed = movementGroundSpeed;
            movementTargetGroundSpeed *= IsSprinting ? movementSprintSpeedMultiplier : 1;
            movementTargetGroundSpeed *= IsCrouching ? movementCrouchSpeedMultiplier : 1;
            characterOrigin.position += 2 * Time.deltaTime * movementTargetGroundSpeed * movementDirection;
        }
        private void MoveAir()
        {
            movementVelocity.y -= movementGravity * Time.deltaTime;

            if (CollisionSides && !isClipResolved)
            {
                ApplyClipVelocity(collisionSidesInfo.normal);
                isClipResolved = true;
            }

            ApplyAirAcceleration(movementAirAccelerate, movementAirSpeed);
        }
        private void MoveSurf()
        {
            // Şimdi bazen rampadan çıkarken hız kesiliyor bunun nedeni groundCheck çok uzun ama işte groundCheck kısaltırsak bu sefer jitter oluşuyor.

            movementVelocity.y -= movementGravity * Time.deltaTime;

            if (!isClipResolved)
            {
                ApplyClipVelocity(collisionGroundInfo.normal);
                isClipResolved = true;
            }

            ApplyAirAcceleration(movementAirAccelerate, movementAirSpeed * 1.25f);
        }
        private void MoveGround()
        {
            bool movingBackwards = Move.GetAxis().y < 0;

            movementTargetGroundSpeed = movementGroundSpeed;
            movementTargetGroundSpeed *= IsSprinting ? movementSprintSpeedMultiplier : 1;
            movementTargetGroundSpeed *= IsCrouching ? movementCrouchSpeedMultiplier : 1;
            movementTargetGroundSpeed *= IsWalking ? movementWalkSpeedMultiplier : 1;
            movementTargetGroundSpeed *= movingBackwards ? movementBackwardsSpeedMultiplier : 1;

            movementCurrentGroundSpeed = Mathf.Lerp(movementCurrentGroundSpeed, movementTargetGroundSpeed, movementSpeedAccelerate * Time.deltaTime);

            float d = movementGroundSpeed * movementSprintSpeedMultiplier;
            movementNormalizedGroundSpeed = Mathf.Clamp(movementTargetGroundSpeed, 0, d) / d;
          
            movementDirection = Vector3.ProjectOnPlane(movementDirection, collisionGroundInfo.normal).normalized;

            ApplyFriction(wishJump || groundTimer < 0.0333f ? 0 : movementGroundFriction);
            ApplyGroundAcceleration(movementGroundAccelerate, movementCurrentGroundSpeed);

            if (!CollisionSides)
            {
                movementVelocity = Vector3.ProjectOnPlane(movementVelocity, collisionGroundInfo.normal);
            }
            else if (CollisionSides && !isClipResolved)
            {
                ApplyClipVelocity(collisionSidesInfo.normal);
                isClipResolved = true;
            }

            stepInterval = Mathf.Lerp(stepIntervalMax, stepIntervalMin, movementNormalizedGroundSpeed);
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0 && Time.timeScale != 0 && IsMoving)
            {
                TryStep();
                stepTimer = stepInterval;
            }
        }
        private void MoveJump()
        {
            // şimdi şu kodda çok komik bir şey var. Jump esnasında velocity.y += jumpForce diyorum ya şimdi bunu ben slope boost için kullanıyorum fakat böyle yapınca bu sefer coyote jump çalışmıyor :D çünkü düşme hızı jumpForce dan fazla oluyor ve jumpForce siliniyo
            //movementVelocity.y += movementJumpForce;

            movementVelocity.y = Mathf.Max(movementVelocity.y + movementJumpForce, movementJumpForce);

            OnJump?.Invoke();
        }

        private void ApplyClipVelocity(Vector3 normal, float overbounce = 1.05f)
        {
            float backoff = Vector3.Dot(movementVelocity, normal);

            if (backoff >= 0)
            {
                return;
            }

            backoff *= overbounce;

            movementVelocity -= normal * backoff;
        }
        private void ApplyGroundAcceleration(float factor, float maxSpeed)
        {
            float currentSpeed = Vector3.Dot(movementVelocity, movementDirection);
            float addSpeed = maxSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return;
            }

            float accelerationSpeed = factor * Time.deltaTime * maxSpeed;
            accelerationSpeed = Mathf.Min(accelerationSpeed, addSpeed);

            movementVelocity += movementDirection * accelerationSpeed;
        }
        private void ApplyAirAcceleration(float factor, float maxSpeed)
        {
            float currentSpeed = Vector3.Dot(movementVelocity, movementDirection);
            float addSpeed = maxSpeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return;
            }

            float accelerationSpeed = factor * maxSpeed * Time.deltaTime;

            if (accelerationSpeed > addSpeed)
            {
                accelerationSpeed = addSpeed;
            }

            movementVelocity += movementDirection * accelerationSpeed;
        }
        private void ApplyFriction(float factor)
        {
            Vector3 horizontalVelocity = new(movementVelocity.x, 0f, movementVelocity.z);

            float currentSpeed = horizontalVelocity.magnitude;
            float frictionSpeed = factor * currentSpeed * Time.deltaTime;
            float finalSpeed = Mathf.Max(currentSpeed - frictionSpeed, 0f);

            if (currentSpeed > 0f)
            {
                finalSpeed /= currentSpeed;
                horizontalVelocity *= finalSpeed;

                movementVelocity.x = horizontalVelocity.x;
                movementVelocity.z = horizontalVelocity.z;
            }
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
                    // Ground'ı side olarak sayma
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
        private bool CalculateHeadCollision()
        {
            collisionHeadRadius = characterController.radius + collisionCeilingRadiusOffset;

            Vector3 rayOrigin = characterOrigin.position + ((characterController.height / 2) * Vector3.up);
            float rayHeight = (characterController.height / 2) - (collisionHeadRadius - characterController.skinWidth - 0.01f);

            collisionHeadDirection = Vector3.up * (rayHeight + collisionCeilingOffset);
            collisionHeadPosition = rayOrigin;

            return Physics.CheckSphere(collisionHeadPosition + collisionHeadDirection, collisionHeadRadius, collisionMask, QueryTriggerInteraction.Ignore);
        }

        public bool ToggleNoclip()
        {
            isNoclipActive = !isNoclipActive;

            if (isNoclipActive)
            {
                collisionMask = 0;
                characterController.enabled = false;
                movementVelocity.y = 0;
            }
            else
            {
                characterController.enabled = true;
                collisionMask = collisionDefaultMask;
            }

            return isNoclipActive;
        }

        private void TryStep()
        {
            if (Physics.Raycast(characterOrigin.position, Vector3.down, out RaycastHit hit, (GetCharacterHeight() / 2) + 0.125f, stepMask, QueryTriggerInteraction.Collide))
            {
                OnStep?.Invoke(hit);
            }
        }

        public Transform GetCameraOrigin() => cameraPivot;

        public Transform GetCharacterOrigin() => characterOrigin;
        public float GetCharacterHeight() => characterController.height;
        public float GetCharacterWidth() => characterController.skinWidth;
        public float GetCharacterRadius() => characterController.radius;

        private MovementStance GetMovementStance() => movementCurrentStance;
        public void OverrideMovementStance(MovementStance value, bool @override)
        {
            movementCurrentStance = value;
            isStanceOverrided = @override;
        }

        public Vector3 GetMovementDirection() => movementDirection;
        public Vector3 GetMovementVelocity() => movementVelocity;

        public LayerMask GetCollisionMask() => collisionMask;
        public void SetCollisionMask(LayerMask value)
        {
            collisionDefaultMask = value;

            for (int i = 0; i < characterColliders.Length; i++)
            {
                characterColliders[i].includeLayers = collisionDefaultMask;
                characterColliders[i].excludeLayers = ~collisionDefaultMask;
            }

            characterController.includeLayers = collisionDefaultMask;
            characterController.excludeLayers = ~collisionDefaultMask;
        }
        public void ResetCollisionMask() => SetCollisionMask(collisionMask);

        public void ForceMove(Vector3 velocity) => characterController.Move(velocity);
        public void ClearVelocity()
        {
            SetVelocity(Vector3.zero);

            wasGrounded = true;
            wasJumped = false;
        }
        public void SetVelocity(Vector3 velocity) => movementVelocity = velocity;
        public void AddVelocity(Vector3 velocity)
        {
            movementVelocity += velocity;
            wasVelocityAdded = true;
        }

        public float GetFieldOfView() => cameraFieldOfView;
        public void SetFieldOfView(float value)
        {
            cameraFieldOfView = value;

            cameraController.fieldOfView = cameraFieldOfView;
        }

        public float GetSensitivity() => cameraSensitivity;
        public void SetSensitivity(float value) => cameraSensitivity = value;

        public float GetGravity() => movementGravity;
        public void SetGravity(float value) => movementGravity = value;

        public float GetJumpForce() => movementJumpForce;
        public void SetJumpForce(float value) => movementJumpForce = Mathf.Max(0, value);

        public float GetSprintSpeedMult() => movementSprintSpeedMultiplier;
        public void SetSprintSpeedMult(float value) => movementSprintSpeedMultiplier = value;

        public float GetCrouchSpeedMult() => movementCrouchSpeedMultiplier;
        public void SetCrouchSpeedMult(float value) => movementCrouchSpeedMultiplier = value;

        public float GetWalkSpeedMult() => movementWalkSpeedMultiplier;
        public void SetWalkSpeedMult(float value) => movementWalkSpeedMultiplier = value;

        public float GetNormalizedSpeed() => movementNormalizedGroundSpeed;
        public float GetCurrentSpeed() => movementVelocity.ClearY().magnitude;

        public float GetGroundSpeed() => movementGroundSpeed;
        public void SetGroundSpeed(float value) => movementGroundSpeed = Mathf.Max(0, value);

        public float GetGroundFriction() => movementGroundFriction;
        public void SetGroundFriction(float value) => movementGroundFriction = value;

        public float GetGroundAcceleration() => movementGroundAccelerate;
        public void SetGroundAcceleration(float value) => movementGroundAccelerate = value;

        public float GetAirSpeed() => movementAirSpeed;
        public void SetAirSpeed(float value) => movementAirSpeed = value;

        public float GetAirAcceleration() => movementAirAccelerate;
        public void SetAirAcceleration(float value) => movementAirAccelerate = value;

        public void SetSprintEnabled(bool status)
        {
            if (status)
            {
                isSprintEnabled.Enable();
            }
            else
            {
                isSprintEnabled.Disable();
            }
        }
        public bool GetSprintEnabled() => isSprintEnabled.IsEnabled;

        public void SetCrouchEnabled(bool status)
        {
            if (status)
            {
                isCrouchEnabled.Enable();
            }
            else
            {
                isCrouchEnabled.Disable();
            }
        }
        public bool GetCrouchEnabled() => isCrouchEnabled.IsEnabled;

        public void SetJumpEnabled(bool status)
        {
            if (status)
            {
                isJumpEnabled.Enable();
            }
            else
            {
                isJumpEnabled.Disable();
            }
        }
        public bool GetJumpEnabled() => isJumpEnabled.IsEnabled;

        public void SetIsInputEnabled(bool status)
        {
            if (status)
            {
                isInputEnabled.Enable();
            }
            else
            {
                isInputEnabled.Disable();
            }
        }
        public bool GetIsInputEnabled() => isInputEnabled.IsEnabled;

        public void SetMovementEnabled(bool status)
        {
            if (status)
            {
                isMovementEnabled.Enable();
            }
            else
            {
                isMovementEnabled.Disable();
                movementVelocity = Vector3.zero;
                fallTimer = 0;
                groundTimer = 0;
                canJump = false;
                canCrouch = false;
                canStand = false;
                canSprint = false;
            }
        }
        public bool GetMovementEnabled() => isMovementEnabled.IsEnabled;

        public void SetLookEnabled(bool status)
        {
            if (status)
            {
                isLookEnabled.Enable();
            }
            else
            {
                isLookEnabled.Disable();
            }
        }
        public bool GetLookEnabled() => isLookEnabled.IsEnabled;
        public void OverrideLookSystem(bool value) => isLookOverrided = value;

        public void SetCollidersEnabled(bool status)
        {
            if (status)
            {
                isCollidersEnabled.Enable();
            }
            else
            {
                isCollidersEnabled.Disable();
            }

            for (int i = 0; i < characterColliders.Length; i++)
            {
                if (!GetCollidersEnabled())
                {
                    characterColliders[i].excludeLayers = ~0;
                    characterColliders[i].includeLayers = 0;
                }
                else
                {
                    characterColliders[i].excludeLayers = ~collisionMask;
                    characterColliders[i].includeLayers = collisionMask;
                }
            }
        }
        public bool GetCollidersEnabled() => isCollidersEnabled.IsEnabled;

        public float GetLookPitch() => cameraRotationX;
        public void SetLookPitch(float value) => cameraRotationX = value;
        public void SetLookPitchClamp(float value) => cameraPitchClampAngle = value;

        public float GetLookYaw() => cameraRotationY;
        public void SetLookYaw(float value) => cameraRotationY = value;
        public void SetLookYawClamp(float value)
        {
            cameraYawClampAngle = Mathf.Max(0f, value);

            if (cameraYawClampAngle > 0f)
            {
                cameraYawClampCenter = cameraRotationY;
            }
        }
    }
}
