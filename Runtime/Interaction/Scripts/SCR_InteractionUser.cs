using UnityEngine;

namespace Core.Interaction
{
    public interface IInteractionUser
    {
        public void HandleInteractionChanged(in InteractionContext ctx);
    }
}