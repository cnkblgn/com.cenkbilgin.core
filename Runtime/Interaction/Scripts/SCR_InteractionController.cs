using System;
using UnityEngine;
using Core.Actors;

namespace Core.Interaction
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Actor))]
    public sealed class InteractionController : MonoBehaviour
    {
        public event Action<InteractionContext> OnStateChanged = null;

        [Header("_")]
        [SerializeField, Required] private Transform origin = null;

        [Header("_")]
        [SerializeField] private LayerMask mask = 0;
        [SerializeField, Min(0.1f)] private float distance = 2.5f;

        private Actor thisActor = null;
        private Interactable currentTarget = null;
        private readonly StackBool canInteract = new();
        private float nextInteractTime;

        private void Awake()
        {
            if (origin == null)
            {
                origin = transform;

                Debug.LogWarning("Interaction origin is missing! assigning default transform to origin!", gameObject);
            }

            thisActor = GetComponent<Actor>();
            nextInteractTime = Time.time + 0.01f;
        }
        private void Update()
        {
            if (!CanProcess())
            {
                return;
            }

            if (TryGetTarget(out Interactable wishTarget))
            {
                UpdateTarget(wishTarget);
            }
            else
            {
                ClearTarget();
            }
        }

        public bool TryInteract()
        {
            if (!CanInteract())
            {
                return false;
            }

            if (currentTarget != null)
            {
                InteractTarget();
            }
            else
            {
                OnStateChanged?.Invoke(new(null, thisActor, null, InteractionState.INTERACT_DENIED));
            }

            return true;
        }

        private bool CanProcess()
        {
            if (!IsInteractionEnabled())
            {
                return false;
            }

            if (origin == null)
            {
                return false;
            }

            if (ManagerGame.Instance == null)
            {
                return false;
            }

            return ManagerGame.Instance.GetGameState() == GameState.RESUME;
        }
        private bool CanInteract()
        {
            if (Time.time < nextInteractTime)
            {
                return false;
            }

            nextInteractTime = Time.time + 0.01f;
            return true;
        }

        private bool TryGetTarget(out Interactable interactable)
        {
            interactable = null;

            if (!Physics.Raycast(origin.position, origin.forward, out RaycastHit hitInfo, distance, mask, QueryTriggerInteraction.Collide))
            {
                return false;
            }

            return hitInfo.collider != null && hitInfo.collider.TryGetComponent(out interactable);
        }
        private void UpdateTarget(Interactable newTarget)
        {
            if (newTarget == currentTarget)
            {
                return;
            }

            ClearTarget();

            currentTarget = newTarget;
            currentTarget.EnterFocus(thisActor, out InteractionContext ctx);
            OnStateChanged?.Invoke(ctx);
        }
        private void InteractTarget()
        {
            if (currentTarget == null)
            {
                OnStateChanged?.Invoke(new(null, thisActor, null, InteractionState.INTERACT_DENIED));
                return;
            }

            currentTarget.Interact(thisActor, out InteractionContext ctx);
            OnStateChanged?.Invoke(ctx);
        }
        private void ClearTarget()
        {
            if (currentTarget == null)
            {
                return;
            }

            currentTarget.ExitFocus(thisActor, out InteractionContext ctx);
            currentTarget = null;

            OnStateChanged?.Invoke(ctx);
        }

        public float GetInteractionDistance() => distance;
        public void SetInteractionDistance(float value) => distance = value;

        public bool IsInteractionEnabled() => canInteract.IsEnabled;
        public void DisableInteraction(object obj)
        {
            canInteract.Disable(obj);

            if (!canInteract.IsEnabled)
            {
                ClearTarget();
            }
        }
        public void EnableInteraction(object obj)
        {
            canInteract.Enable(obj);
        }
    }
}