using System;
using UnityEngine;

namespace Core.UI
{
    internal sealed class UIWaypointPool : IPoolHandler<UIWaypointItem>
    {
        public readonly PoolSystem<UIWaypointItem> Pool;

        public UIWaypointPool(PoolType type, UIWaypointItem prefab, Transform container, int count) => Pool = new("UI_WAYPOINT_POOL", type, prefab, container, count, this);
         
        public UIWaypointItem Spawn(in UIWaypointData data, Vector3 offset)
        {
            UIWaypointItem entity = Pool.GetNext();

            if (entity == null)
            {
                return null;
            }

            entity.Show(data, offset);

            return entity;
        }

        public void HandleInitialization(UIWaypointItem entity) => entity.Initialize();
        public void HandleReset(UIWaypointItem entity) => entity.Deinitialize();
    }
}
