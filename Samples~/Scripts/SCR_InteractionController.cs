using System;
using UnityEngine;
using Core.Input;

namespace Core.Misc
{
    using static CoreUtility;
    using static InputActionDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameEntity))]
    public class InteractionController : MonoBehaviour
    {
        public event Action<Interactable, float> OnInteractProgress = null;
        public event Action<Interactable> OnInteractSuccess = null;
        public event Action<Interactable> OnInteractFailed = null;
        public event Action<Interactable> OnInteractableExitFocus = null;
        public event Action<Interactable> OnInteractableEnterFocus = null;
        public event Action<Interactable> OnInteractableStayFocus = null;

        public bool IsFocusing => isFocusing;
        public float InteractProgress => interactProgress;

        [Header("_")]
        [SerializeField, Required] private Transform interactionTransform = null;

        [Header("_")]
        [SerializeField] private LayerMask interactionMask = 0;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;

        private GameEntity entityObject = null;
        private Interactable interactableObject = null;
        private readonly Flag isEnabled = new();
        private float interactTimer = 0f;
        private float interactProgress = 0f;
        private bool wasInteracted = false;
        private bool wishInteract = false;
        private bool wishHold = false;
        private bool wasInteractSuccess = false;
        private bool wasInteractHold = false;
        private bool canInteract = false;
        private bool canHold = true;
        private bool isExitedFocus = false;
        private bool isFocusing = false;
        private bool isHolding = false;

        private void Awake() => entityObject = GetComponent<GameEntity>();
        private void Update()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (!isEnabled.Value)
            {
                return;
            }

            interactTimer += Time.deltaTime;
            wishHold = ManagerCoreInput.Instance.GetButton(Interact);
            wishInteract = ManagerCoreInput.Instance.GetButtonDown(Interact);
            canInteract = wishInteract && interactTimer > 0.01f;
            canHold = wishHold && !wasInteractHold;

            if (wasInteractHold && ManagerCoreInput.Instance.GetButtonUp(Interact))
            {
                wasInteractHold = false;
            }
            if (isHolding)
            {
                if (ManagerCoreInput.Instance.GetButtonUp(Interact))
                {
                    isHolding = false;

                    if (interactableObject != null && interactableObject.Type == InteractableType.HOLD_DEFAULT)
                    {
                        interactableObject.SetInteractionProgress(interactProgress = 0);
                    }
                }
            }

            if (canInteract)
            {
                wasInteracted = false;
            }
            else
            {
                if (!wasInteracted)
                {
                    interactTimer = 0;
                }

                wasInteracted = true;
            }

            isFocusing = Physics.Raycast(interactionTransform.position, interactionTransform.forward, out RaycastHit hitInfo, interactionDistance, interactionMask, QueryTriggerInteraction.Collide);

            if (isFocusing)
            {
                if (interactableObject == null)
                {
                    if (wasInteractSuccess)
                    {
                        OnInteractableExitFocus?.Invoke(interactableObject);
                        wasInteractSuccess = false;
                    }

                    if (!hitInfo.collider.TryGetComponent(out interactableObject))
                    {
                        if (canInteract)
                        {
                            OnInteractFailed?.Invoke(null);
                        }

                        return;
                    }

                    interactableObject.OnEnterFocus(entityObject);
                    OnInteractableEnterFocus?.Invoke(interactableObject);
                    isExitedFocus = false;
                }
                else
                {
                    if (hitInfo.collider.gameObject.GetInstanceID() != interactableObject.gameObject.GetInstanceID())
                    {
                        OnInteractableExitFocus?.Invoke(interactableObject);
                        interactableObject.OnExitFocus(entityObject);
                        interactableObject = null;
                    }
                    else
                    {
                        OnInteractableStayFocus?.Invoke(interactableObject);
                        interactableObject.OnStayFocus(entityObject);

                        if (interactableObject.Type == InteractableType.DEFAULT)
                        {
                            if (canInteract)
                            {
                                if (interactableObject.OnInteract(entityObject))
                                {
                                    wasInteractSuccess = true;
                                    OnInteractSuccess?.Invoke(interactableObject);
                                }
                                else
                                {
                                    OnInteractFailed?.Invoke(interactableObject);
                                }
                            }
                        }
                        else if (interactableObject.Type == InteractableType.HOLD_DEFAULT || interactableObject.Type == InteractableType.HOLD_PERSISTENT)
                        {
                            if (canHold)
                            {
                                interactProgress = interactableObject.OnProgress(entityObject);

                                if (interactProgress >= 0)
                                {
                                    OnInteractProgress?.Invoke(interactableObject, interactProgress);

                                    // Completed hold interaction
                                    if (interactProgress >= 1f)
                                    {
                                        if (interactableObject.OnInteract(entityObject))
                                        {
                                            wasInteractSuccess = true;
                                            OnInteractSuccess?.Invoke(interactableObject);
                                        }
                                        else
                                        {
                                            OnInteractFailed?.Invoke(interactableObject);
                                        }

                                        interactableObject.SetInteractionProgress(interactProgress = 0);
                                        wasInteractHold = true;
                                    }

                                    isHolding = true;
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("InteractionController interactableObject.Type == null");
                        }
                    }
                }
            }
            else
            {
                if (canInteract)
                {
                    OnInteractFailed?.Invoke(null);
                }

                if (interactableObject != null)
                {
                    OnInteractableExitFocus?.Invoke(interactableObject);
                    interactableObject.OnExitFocus(entityObject);
                    interactableObject = null;
                }
                else
                {
                    if (!isExitedFocus)
                    {
                        OnInteractableExitFocus?.Invoke(null);
                        isExitedFocus = true;
                    }
                }
            }
        }

        public float GetInteractionDistance() => interactionDistance;
        public void SetInteractionDistance(float value) => interactionDistance = value;

        public void SetInteractionEnabled(bool status, object requester)
        {
            if (requester == null)
            {
                Debug.LogError("Requester is null!", this.gameObject);
                return;
            }

            if (status)
            {
                isEnabled.Enable(requester);
            }
            else
            {
                isEnabled.Disable(requester);

                if (interactableObject != null)
                {
                    OnInteractableExitFocus?.Invoke(interactableObject);
                    interactableObject.OnExitFocus(entityObject);
                    interactableObject = null;
                }
            }
        }
        public bool GetInteractionEnabled() => isEnabled.Value;
    }
}
