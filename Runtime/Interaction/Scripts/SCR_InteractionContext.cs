using UnityEngine;

namespace Core.Interaction
{
    public readonly struct InteractionContext
    {
        public readonly Interactable Interactable;
        public readonly GameObject Interactor;
        public readonly IInteractionAction Action;
        public readonly InteractionState State;

        public InteractionContext(Interactable interactable, GameObject interactor, IInteractionAction action, InteractionState state)
        {
            Interactable = interactable;
            Interactor = interactor;
            Action = action;
            State = state;
        }
    }
}
