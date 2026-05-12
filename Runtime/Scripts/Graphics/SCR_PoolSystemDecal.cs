using UnityEngine;

namespace Core.Graphics
{
    public sealed class PoolSystemDecal : PoolSystem<DecalEmitter>
    {
        public override PoolType Type => PoolType.RING_BUFFER;
        public override string ID => "DECAL_POOL";

        protected sealed override void OnInitialize(DecalEmitter item) { }
        protected sealed override void OnReset(DecalEmitter item)
        {
            if (item == null)
            {
                Debug.LogError("item == null! possible illegal destroyed decal");
                return;
            }

            item.ThisTransform.SetParent(thisContainer);
            item.gameObject.SetActive(false);
        }

        public DecalEmitter Spawn(Transform parent, Vector3 position, Quaternion rotation, float scale)
        {
            DecalEmitter decalEmitter = GetNext();

            if (decalEmitter == null)
            {
                Debug.LogError("decalEmitter == null! possible illegal destroyed decal");
                return null;
            }

            decalEmitter.gameObject.SetActive(true);
            decalEmitter.Adjust(position, rotation, scale, parent);

            return decalEmitter;
        }
    }
}