using UnityEngine;

namespace Core.Interaction
{
    public interface IInteractionUser
    {
        public void HandleStateChanged(in InteractionContext ctx);
    }
}