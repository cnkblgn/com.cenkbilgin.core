using System;
using UnityEngine;

namespace Core.UI
{
    internal sealed class UIWaypointPool : IPoolHandler<UIWaypointView>
    {
        public readonly PoolSystem<UIWaypointView> Pool;

        public UIWaypointPool(PoolType type, UIWaypointView prefab, Transform container, int count) => Pool = new("UI_WAYPOINT_POOL", type, prefab, container, count, this);
         
        public UIWaypointView Spawn(in UIWaypointData data, Vector3 offset, Func<bool> destroyUntil)
        {
            UIWaypointView entity = Pool.GetNext();

            if (entity == null)
            {
                return null;
            }

            entity.Show(data, offset, destroyUntil);

            return entity;
        }

        public void HandleInitialization(UIWaypointView entity) => entity.Initialize();
        public void HandleReset(UIWaypointView entity) => entity.Deinitialize();
    }
}
