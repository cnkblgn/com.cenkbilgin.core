using System;
using UnityEngine;

namespace Core.UI
{
    public class PoolSystemUIWaypoint : PoolSystem<UIWaypointEntity>
    {
        public override PoolType Type => PoolType.RELEASE;
        public override string ID => "WAYPOINT_POOL";

        protected override void OnInitialize(UIWaypointEntity item) => item.Initialize();
        protected override void OnReset(UIWaypointEntity item) => item.Deinitialize();

        public UIWaypointEntity Spawn(Transform targetTransform, Vector3 targetOffset, Sprite iconSprite, Color iconColor, string iconText, float duration, Func<bool> destroyUntil)
        {
            UIWaypointEntity waypointObject = GetNext();

            if (waypointObject == null)
            {
                return null;
            }

            waypointObject.Show(targetTransform, targetOffset, iconSprite, iconColor, iconText, duration, destroyUntil);
            return waypointObject;
        }
    }
}
