using System;
using UnityEngine;
using Core.Actors;
using Core.Localization;

namespace Core.Interaction
{
    using static CoreUtility;

    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour
    {
        public event Action<InteractionContext> OnStateChanged = null;

        public string Description => description;
        public Collider ThisCollider => thisCollider;
        public Transform ThisTransform => thisTransform;

        [Header("_")]
        [SerializeField] private LocalizedID descriptionID = default;
        [SerializeField] private string descriptionStyles = "info_white";

        private Transform thisTransform;
        private Collider thisCollider;
        private IInteractableHandler[] thisHandlers;
        private string description = STRING_EMPTY;
        private bool isEnabled = true;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
            thisTransform = transform;
            thisHandlers = GetComponents<IInteractableHandler>();

            if (descriptionID.IsValid)
            {
                OverrideDescription(descriptionID.Get());
            }
        }

        private void SetState(in InteractionContext ctx)
        {
            for (int i = 0; i < thisHandlers.Length; i++)
            {
                thisHandlers[i].HandleStateChanged(ctx);
            }

            OnStateChanged?.Invoke(ctx);
        }

        public void EnterFocus(Actor entity, out InteractionContext ctx)
        {
            ctx = new(this, entity, null, InteractionState.FOCUS_ENTER);

            SetState(ctx);
        }
        public void ExitFocus(Actor entity, out InteractionContext ctx)
        {
            ctx = new(this, entity, null, InteractionState.FOCUS_EXIT);

            SetState(ctx);
        }
        public void Interact(Actor entity, out InteractionContext ctx)
        {
            IInteractionAction action = null;

            if (!isEnabled)
            {
                ctx = new(this, entity, action, InteractionState.INTERACT_DENIED);
                SetState(ctx);
                return;
            }

            for (int i = 0; i < thisHandlers.Length; i++)
            {
                if (!thisHandlers[i].HandleInteract(entity, out IInteractionAction handlerAction))
                {
                    ctx = new(this, entity, action, InteractionState.INTERACT_DENIED);
                    SetState(ctx);
                    return;
                }

                action ??= handlerAction;
            }

            ctx = new(this, entity, action, InteractionState.INTERACT_ACCEPTED);
            SetState(ctx);
        }

        public void EnableInteraction()
        {
            isEnabled = true;
            thisCollider.enabled = true;
        }
        public void DisableInteraction()
        {
            isEnabled = false;
            thisCollider.enabled = false;
        }

        public void OverrideDescription(string value)
        {
            description = string.IsNullOrEmpty(descriptionStyles) ? value : value.ToStyle(descriptionStyles);
        }
    }
}