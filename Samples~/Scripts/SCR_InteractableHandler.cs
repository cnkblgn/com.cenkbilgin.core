using Core;
using UnityEngine;

namespace Game
{
    public interface IInteractableHandler
    {
        public InteractableType Type { get; }
        public void HandleEnterFocus(GameEntity entity);
        public void HandleExitFocus(GameEntity entity);
        public bool HandleInteract(GameEntity entity);
    }
}