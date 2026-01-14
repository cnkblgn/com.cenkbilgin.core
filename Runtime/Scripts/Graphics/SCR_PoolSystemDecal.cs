using System;
using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [Serializable]
    public class PoolSystemDecal : PoolSystem<DecalEmitter>
    {
        protected override void OnInitialize(DecalEmitter item) { }
        protected override void OnReset(DecalEmitter item)
        {
            if (item == null)
            {
                // also maybe restore from prefabDatabase via ID?
                Debug.LogError("PoolSystemDecal.OnReset() item == null! possible illegal destroyed decal");
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
                Debug.LogError("PoolSystemDecal.Spawn() decalEmitter == null! possible illegal destroyed decal");
                return null;
            }

            decalEmitter.gameObject.SetActive(true);
            decalEmitter.Adjust(position, rotation, scale, parent);

            return decalEmitter;
        }
    }
}