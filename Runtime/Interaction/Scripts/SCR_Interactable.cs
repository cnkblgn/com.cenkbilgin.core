using System;
using UnityEngine;

namespace Core.Interaction
{
    using static CoreUtility;

    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class Interactable : MonoBehaviour
    {
        public event Action<InteractionContext> OnStateChanged = null;

        public string Label => label; private string label = STRING_EMPTY;

        private Collider thisCollider;
        private IInteractableHandler[] thisHandlers;      
        private bool isEnabled = true;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
            thisHandlers = GetComponents<IInteractableHandler>();
        }
        private void OnDisable() => ExitFocus(null, out _);

        private void SetState(in InteractionContext ctx)
        {
            for (int i = 0; i < thisHandlers.Length; i++)
            {
                thisHandlers[i].HandleStateChanged(ctx);
            }

            OnStateChanged?.Invoke(ctx);
        }

        public void EnterFocus(GameObject interactor, out InteractionContext ctx)
        {
            ctx = new(this, interactor, null, InteractionState.FOCUS_ENTER);

            SetState(ctx);
        }
        public void ExitFocus(GameObject interactor, out InteractionContext ctx)
        {
            ctx = new(this, interactor, null, InteractionState.FOCUS_EXIT);

            SetState(ctx);
        }
        public void Interact(GameObject interactor, out InteractionContext ctx)
        {
            IInteractionAction action = null;

            if (!isEnabled)
            {
                ctx = new(this, interactor, action, InteractionState.INTERACT_DENIED);
                SetState(ctx);
                return;
            }

            for (int i = 0; i < thisHandlers.Length; i++)
            {
                if (!thisHandlers[i].HandleInteract(interactor, out IInteractionAction handlerAction))
                {
                    ctx = new(this, interactor, action, InteractionState.INTERACT_DENIED);
                    SetState(ctx);
                    return;
                }

                action ??= handlerAction;
            }

            ctx = new(this, interactor, action, InteractionState.INTERACT_ACCEPTED);
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

        public void OverrideLabel(string value) => label = value;
    }
}