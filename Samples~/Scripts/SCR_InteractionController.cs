using System;
using UnityEngine;
using Core;
using Core.Input;

namespace Game
{
    using static CoreUtility;
    using static InputDatabase;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameEntity))]
    public sealed class InteractionController : MonoBehaviour
    {
        public event Action<InteractableEntity> OnSuccess = null;
        public event Action<InteractableEntity> OnFailed = null;
        public event Action<InteractableEntity> OnExitFocus = null;
        public event Action<InteractableEntity> OnEnterFocus = null;

        [Header("_")]
        [SerializeField, Required] private Transform interactionTransform = null;

        [Header("_")]
        [SerializeField] private LayerMask interactionMask = 0;
        [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;

        private GameEntity thisEntity = null;
        private InteractableEntity interactableEntity = null;
        private readonly StackBool isInputEnabled = new(8);
        private float interactTimer = 0f;
        private bool wasInteracted = false;
        private bool wishInteract = false;
        private bool wasInteractSuccess = false;
        private bool canInteract = false;
        private bool isExitedFocus = false;
        private bool isFocusing = false;

        private void Awake() => thisEntity = GetComponent<GameEntity>();
        private void Update()
        {
            if (ManagerCoreGame.Instance.GetGameState() != GameState.RESUME)
            {
                return;
            }

            if (!isInputEnabled.IsEnabled)
            {
                return;
            }

            interactTimer += Time.deltaTime;
            wishInteract = Interact.GetKeyDown();
            canInteract = wishInteract && interactTimer > 0.01f;

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
                if (interactableEntity == null)
                {
                    if (wasInteractSuccess)
                    {
                        OnExitFocus?.Invoke(interactableEntity);
                        wasInteractSuccess = false;
                    }

                    if (!hitInfo.collider.TryGetComponent(out interactableEntity))
                    {
                        if (canInteract)
                        {
                            OnFailed?.Invoke(null);
                        }

                        return;
                    }

                    interactableEntity.OnEnterFocus(thisEntity);
                    OnEnterFocus?.Invoke(interactableEntity);
                    isExitedFocus = false;
                }
                else
                {
                    if (hitInfo.collider.gameObject.GetEntityId() != interactableEntity.gameObject.GetEntityId())
                    {
                        OnExitFocus?.Invoke(interactableEntity);
                        interactableEntity.OnExitFocus(thisEntity);
                        interactableEntity = null;
                    }
                    else
                    {
                        if (canInteract)
                        {
                            if (interactableEntity.OnInteract(thisEntity))
                            {
                                wasInteractSuccess = true;
                                OnSuccess?.Invoke(interactableEntity);
                            }
                            else
                            {
                                OnFailed?.Invoke(interactableEntity);
                            }
                        }
                    }
                }
            }
            else
            {
                if (canInteract)
                {
                    OnFailed?.Invoke(null);
                }

                if (interactableEntity != null)
                {
                    OnExitFocus?.Invoke(interactableEntity);
                    interactableEntity.OnExitFocus(thisEntity);
                    interactableEntity = null;
                }
                else
                {
                    if (!isExitedFocus)
                    {
                        OnExitFocus?.Invoke(null);
                        isExitedFocus = true;
                    }
                }
            }
        }

        public float GetInteractionDistance() => interactionDistance;
        public void SetInteractionDistance(float value) => interactionDistance = value;

        public void DisableInput(out int token)
        {
            isInputEnabled.Disable(out token);

            if (!isInputEnabled.IsEnabled && interactableEntity != null)
            {
                OnExitFocus?.Invoke(interactableEntity);
                interactableEntity.OnExitFocus(thisEntity);
                interactableEntity = null;
            }
        }
        public void EnableInput(ref int token)
        {
            isInputEnabled.Enable(ref token);
        }
    }
}
