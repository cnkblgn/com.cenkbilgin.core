using UnityEngine;

namespace Core.Graphics
{
    public sealed class DecalPool : IPoolHandler<DecalEmitter>
    {
        public readonly PoolSystem<DecalEmitter> Pool;
  
        public DecalPool(PoolType type, DecalEmitter prefab, Transform container, int count) => Pool = new("DECAL_POOL", type, prefab, container, count, this);

        public DecalEmitter Spawn(Transform parent, Vector3 position, Quaternion rotation, float scale)
        {
            DecalEmitter decalEmitter = Pool.GetNext();

            if (decalEmitter == null)
            {
                Debug.LogError("decalEmitter == null! possible illegal destroyed decal");
                return null;
            }

            decalEmitter.gameObject.SetActive(true);
            decalEmitter.Adjust(position, rotation, scale, parent);

            return decalEmitter;
        }

        public void OnInitialize(DecalEmitter item) { }
        public void OnReset(DecalEmitter item) { }
    }
}
