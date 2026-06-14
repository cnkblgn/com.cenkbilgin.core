using UnityEngine;

namespace Game
{
    public readonly struct MovementContext
    {
        public readonly MovementState State;
        public readonly Collider CollidedCollider;
        public readonly Vector3 Velocity;
        public readonly RaycastHit Step;
        public readonly float FallTime;
        public readonly float FallSpeed;
        public readonly float FallHeight;

        public MovementContext(MovementState state, Collider collidedCollider, Vector3 velocity, RaycastHit step, float fallTime, float fallSpeed, float fallHeight)
        {
            State = state;
            CollidedCollider = collidedCollider;
            Velocity = velocity;
            Step = step;
            FallTime = fallTime;
            FallSpeed = fallSpeed;
            FallHeight = fallHeight;
        }
    }
}
