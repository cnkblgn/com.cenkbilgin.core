using UnityEngine;

namespace Core.Animation
{
    public struct SpringState
    {
        public bool IsActive { get; private set; }

        private Vector3 currentValue;
        private Vector3 currentVelocity;
        private float frequency;
        private float damping;
        private const float EPS = 0.0001f;
        private const float FREQ_EPS = 1e-5f;

        public void Start(SpringConfig config, float strength)
        {
            Vector3 amplitude = config.Amplitude * strength;

            if (amplitude.sqrMagnitude < EPS)
            {
                return;
            }

            currentVelocity += amplitude;
            frequency = Mathf.Max(0, config.Frequency);
            damping = Mathf.Clamp01(config.Damping);
            IsActive = true;
        }

        public Vector3 Update(float deltaTime)
        {
            if (!IsActive)
            {
                return currentValue;
            }

            float w = frequency;
            float z = damping;

            if (w < FREQ_EPS)
            {
                currentValue += currentVelocity * deltaTime;
            }
            else
            {
                float a = z * w;
                Vector3 x0 = currentValue;
                Vector3 v0 = currentVelocity;
                float expTerm = Mathf.Exp(-a * deltaTime);

                if (z < 0.999f)
                {
                    float wd = w * Mathf.Sqrt(1f - z * z);
                    float cosT = Mathf.Cos(wd * deltaTime);
                    float sinT = Mathf.Sin(wd * deltaTime);

                    Vector3 B = (v0 + a * x0) / wd;

                    Vector3 newValue = expTerm * (x0 * cosT + B * sinT);
                    Vector3 C = (a * (v0 + a * x0) / wd) + x0 * wd;
                    Vector3 newVelocity = expTerm * (v0 * cosT - C * sinT);

                    currentValue = newValue;
                    currentVelocity = newVelocity;
                }
                else
                {
                    Vector3 newValue = expTerm * (x0 + (v0 + a * x0) * deltaTime);
                    Vector3 newVelocity = expTerm * (v0 - a * deltaTime * (v0 + a * x0));

                    currentValue = newValue;
                    currentVelocity = newVelocity;
                }
            }

            if (currentValue.sqrMagnitude < EPS && currentVelocity.sqrMagnitude < EPS)
            {
                currentValue = Vector3.zero;
                currentVelocity = Vector3.zero;
                IsActive = false;
            }

            return currentValue;
        }
    }
}