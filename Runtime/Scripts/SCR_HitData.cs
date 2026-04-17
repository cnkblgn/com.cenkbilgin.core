using UnityEngine;

namespace Core
{
    public readonly struct HitData
    {
        public readonly Collider Collider;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly float Distance;

        public HitData(Collider collider, Vector3 point, Vector3 normal, float distance)
        {
            Collider = collider;
            Point = point;
            Normal = normal;
            Distance = distance;
        }
    }
}
