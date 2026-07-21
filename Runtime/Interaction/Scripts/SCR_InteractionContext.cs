using Core.Actors;

namespace Core.Interaction
{
    public readonly struct InteractionContext
    {
        public readonly Interactable Interactable;
        public readonly Actor Interactor;
        public readonly IInteractionAction Action;
        public readonly InteractionState State;

        public InteractionContext(Interactable interactable, Actor interactor, IInteractionAction action, InteractionState state)
        {
            Interactable = interactable;
            Interactor = interactor;
            Action = action;
            State = state;
        }
    }
}
