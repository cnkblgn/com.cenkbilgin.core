using UnityEngine;

namespace Core.Animation
{
    public class PoseInstance
    {
        private Vector3 currentVelocity = Vector3.zero;
        public Vector3 Update(Vector3 from, PoseConfig target, float deltaTime)
        {
            if (target == null)
            {
                return Vector3.SmoothDamp(from, Vector3.zero, ref currentVelocity, 2.5f, float.PositiveInfinity, deltaTime);
            }

            return Vector3.SmoothDamp(from, target.Target, ref currentVelocity, target.Smoothness, float.PositiveInfinity, deltaTime);
        }
    }
}