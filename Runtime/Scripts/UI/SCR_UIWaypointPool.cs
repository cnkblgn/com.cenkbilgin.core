using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class UIWaypointPool : IPoolHandler<UIWaypoint>
    {
        public readonly PoolSystem<UIWaypoint> Pool;

        public UIWaypointPool(PoolType type, UIWaypoint prefab, Transform container, int count) => Pool = new("UI_WAYPOINT_POOL", type, prefab, container, count, this);
         
        public UIWaypoint Spawn(in UIWaypointData data, Vector3 offset, Func<bool> destroyUntil)
        {
            UIWaypoint entity = Pool.GetNext();

            if (entity == null)
            {
                return null;
            }

            entity.Show(data, offset, destroyUntil);

            return entity;
        }

        public void HandleInitialization(UIWaypoint entity) => entity.Initialize();
        public void HandleReset(UIWaypoint entity) => entity.Deinitialize();
    }
}
