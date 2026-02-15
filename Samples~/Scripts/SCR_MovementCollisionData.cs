using Unity.Mathematics;
using UnityEngine;

namespace Game
{
    public readonly struct MovementCollisionData
    {
        public readonly Collider Collider;
        public readonly float3 Velocity;

        public MovementCollisionData(Collider collider, float3 velocity)
        {
            Collider = collider;
            Velocity = velocity;
        }
    }
}
