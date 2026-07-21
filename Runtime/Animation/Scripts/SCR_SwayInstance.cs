using UnityEngine;

namespace Core.Animation
{
    public class SwayInstance
    {
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 currentValue = Vector3.zero;
        private Vector3 targetValue = Vector3.zero;

        public Vector3 Update(SwayConfig config, float inputX, float inputY, float inputZ, float deltaTime, float strength = 1)
        {
            if (config == null)
            {
                return Vector3.zero;
            }

            float x = Mathf.Clamp((inputX * config.Amplitude.x * strength) + (config.IsOffset.x ? currentValue.x : 0), -config.Clamp.x, config.Clamp.x);
            float y = Mathf.Clamp((inputY * config.Amplitude.y * strength) + (config.IsOffset.y ? currentValue.y : 0), -config.Clamp.y, config.Clamp.y);
            float z = Mathf.Clamp((inputZ * config.Amplitude.z * strength) + (config.IsOffset.z ? currentValue.z : 0), -config.Clamp.z, config.Clamp.z);

            targetValue.x = float.IsNaN(x) ? 0 : x;
            targetValue.y = float.IsNaN(y) ? 0 : y;
            targetValue.z = float.IsNaN(z) ? 0 : z;

            return currentValue = Vector3.SmoothDamp(currentValue, targetValue, ref currentVelocity, config.Smoothness, float.PositiveInfinity, deltaTime);
        }
    }
}