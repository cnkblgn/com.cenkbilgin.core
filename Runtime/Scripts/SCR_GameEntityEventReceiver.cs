using System;
using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameEntity))]
    public class GameEntityEventReceiver : MonoBehaviour
    {
        public event Action<GameEntityEventEmitter> OnEventReceived = null;

        private GameEntity thisEntity = null;

        private void Start() => thisEntity = GetComponent<GameEntity>();
        public void Receive(GameEntityEventEmitter eventEmitter)
        {
            if (!thisEntity.enabled)
            {
                return;
            }

            OnEventReceived?.Invoke(eventEmitter);
        }
    }
}