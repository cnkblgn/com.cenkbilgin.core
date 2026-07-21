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

            Vector3 acceleration = -frequency * frequency * currentValue - 2f * damping * frequency * currentVelocity;
            currentVelocity += acceleration * deltaTime;
            currentValue += currentVelocity * deltaTime;

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