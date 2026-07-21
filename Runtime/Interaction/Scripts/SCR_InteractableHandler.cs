using Core.Actors;

namespace Core.Interaction
{
    public interface IInteractableHandler
    {
        /// <summary> Handles interaction. Return false if interaction should be denied. Optionally create a PlayerAction. </summary>
        public bool HandleInteract(Actor entity, out IInteractionAction action);
        /// <summary> Called when interaction state changed. </summary>
        public void HandleStateChanged(in InteractionContext ctx);
    }
}