using UnityEngine;

namespace Core.Interaction
{
    public interface IInteractionHandler
    {
        public void HandleInteractionChanged(in InteractionContext ctx);
    }
}