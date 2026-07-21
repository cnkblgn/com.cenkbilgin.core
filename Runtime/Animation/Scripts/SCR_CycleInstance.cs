using UnityEngine;

namespace Core.Animation
{
    public sealed class CycleInstance
    {
        private Vector3 currentValue = Vector3.zero;
        private Vector3 targetValue = Vector3.zero;

        public Vector3 Update(CycleConfig config, float deltaTime, float strength = 1)
        {
            if (config == null)
            {
                return Vector3.zero;
            }

            float x = config.Amplitude.x * Mathf.Cos(Time.time * config.Frequency.x * Mathf.PI * 2f * 0.5f);
            float y = config.Amplitude.y * Mathf.Sin(Time.time * config.Frequency.y * Mathf.PI * 2f);
            float z = config.Amplitude.z * Mathf.Sin(Time.time * config.Frequency.z * Mathf.PI * 2f);

            targetValue.x = Mathf.Clamp(x * strength, -config.Clamp.x, config.Clamp.x);
            targetValue.y = Mathf.Clamp(y * strength, -config.Clamp.y, config.Clamp.y);
            targetValue.z = Mathf.Clamp(z * strength, -config.Clamp.z, config.Clamp.z);

            float t = 1f - Mathf.Exp(-config.Roughness * deltaTime);

            return currentValue = Vector3.Lerp(currentValue, targetValue, t);
        }
    }
}