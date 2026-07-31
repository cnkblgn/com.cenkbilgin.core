using UnityEngine;

namespace Core.Animation
{
    public sealed class PoseInstance
    {
        private Vector3 currentPose = Vector3.zero;
        private Vector3 currentVelocity = Vector3.zero;

        public Vector3 Update(PoseConfig target, float deltaTime)
        {
            Vector3 targetValue = target != null ? target.Target : Vector3.zero;
            float smoothness = target != null ? target.Smoothness : 2.5f;

            currentPose = Vector3.SmoothDamp(currentPose, targetValue, ref currentVelocity, smoothness, 100f, deltaTime * 10f);

            return currentPose;
        }

        public void Reset(Vector3 value = default)
        {
            currentPose = value;
            currentVelocity = Vector3.zero;
        }
    }
}