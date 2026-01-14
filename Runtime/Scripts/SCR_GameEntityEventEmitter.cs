using UnityEngine;

namespace Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameEntity))]
    public class GameEntityEventEmitter : MonoBehaviour
    {
        private const float EMIT_INTERVAL = 1F;

        private Collider[] thisColliders = null;
        private Transform thisTransform = null;
        private GameEntity thisEntity = null;
        private float currentTime = 0f;
        private int emitMask = -1;

        private void Start()
        {
            thisEntity = GetComponent<GameEntity>();
            thisTransform = GetComponent<Transform>();
            emitMask = (1 << thisEntity.gameObject.layer);
        }
        public void Emit(float emitRadius)
        {
            if ((Time.realtimeSinceStartup - currentTime < EMIT_INTERVAL) && currentTime >= EMIT_INTERVAL)
            {
                return;
            }

            currentTime = Time.realtimeSinceStartup;

            thisColliders = Physics.OverlapSphere(thisTransform.position, emitRadius, emitMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < thisColliders.Length; i++)
            {
                if (gameObject == thisColliders[i].gameObject)
                {
                    continue;
                }

                if (!thisColliders[i].TryGetComponent(out GameEntityEventReceiver eventReceiver))
                {
                    continue;
                }

                eventReceiver.Receive(this);
            }
        }
    }
}