using UnityEngine;

namespace Core.Surface
{
    public readonly struct SurfaceContext
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Collider Collider;

        public SurfaceContext(Vector3 position, Vector3 normal, Collider collider)
        {
            Position = position;
            Normal = normal;
            Collider = collider;
        }
    }
}
