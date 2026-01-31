using Unity.Mathematics;
using UnityEngine;

namespace Core.Misc
{
    public readonly struct MovememntCollisionData
    {
        public readonly Collider Collider;
        public readonly float3 Velocity;

        public MovememntCollisionData(Collider collider, float3 velocity)
        {
            Collider = collider;
            Velocity = velocity;
        }
    }
}
