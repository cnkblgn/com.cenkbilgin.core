using System;
using UnityEngine;

namespace Core.UI
{
    public sealed class UIWaypointPool : IPoolHandler<UIWaypointEntity>
    {
        public readonly PoolSystem<UIWaypointEntity> Pool;

        public UIWaypointPool(PoolType type, UIWaypointEntity prefab, Transform container, int count) => Pool = new("UI_WAYPOINT_POOL", type, prefab, container, count, this);

        public UIWaypointEntity Spawn(Transform target, Vector3 offset, Sprite sprite, Color color, string text, float duration, Func<bool> destroyUntil)
        {
            UIWaypointEntity waypointObject = Pool.GetNext();

            if (waypointObject == null)
            {
                return null;
            }

            waypointObject.Show(target, offset, sprite, color, text, duration, destroyUntil);
            return waypointObject;
        }

        public void OnInitialize(UIWaypointEntity entity) => entity.Initialize();
        public void OnReset(UIWaypointEntity entity) => entity.Deinitialize();
    }
}
