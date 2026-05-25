using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class UIWaypointPool : IPoolHandler<UIWaypointEntity>
    {
        public readonly PoolSystem<UIWaypointEntity> Pool;

        public UIWaypointPool(PoolType type, UIWaypointEntity prefab, Transform container, int count) => Pool = new("UI_WAYPOINT_POOL", type, prefab, container, count, this);
         
        public UIWaypointEntity Spawn(UIWaypointData data, Vector3 offset, Func<bool> destroyUntil)
        {
            UIWaypointEntity entity = Pool.GetNext();

            if (entity == null)
            {
                return null;
            }

            entity.Show(data, offset, destroyUntil);

            return entity;
        }

        public void OnInitialize(UIWaypointEntity entity) => entity.Initialize();
        public void OnReset(UIWaypointEntity entity) => entity.Deinitialize();
    }
}
