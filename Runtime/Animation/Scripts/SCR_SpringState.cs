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

        // Esikler MAGNITUDE cinsinden tanimli, karesi ayrica alinir.
        // Kullandigin birime gore (metre / derece) ayarlayabilirsin.
        private const float POS_EPS = 1e-4f;
        private const float VEL_EPS = 1e-3f;
        private const float START_EPS = 1e-5f;

        private const float POS_EPS_SQR = POS_EPS * POS_EPS;
        private const float VEL_EPS_SQR = VEL_EPS * VEL_EPS;
        private const float START_EPS_SQR = START_EPS * START_EPS;

        private const float FREQ_EPS = 1e-5f;
        private const float MAX_DELTA_TIME = 0.05f;

        public void Start(SpringConfig config, float strength)
        {
            Vector3 amplitude = config.Amplitude * strength;

            if (amplitude.sqrMagnitude < START_EPS_SQR)
            {
                return;
            }

            currentVelocity += amplitude;
            frequency = Mathf.Max(0f, config.Frequency);
            damping = Mathf.Clamp01(config.Damping);
            IsActive = true;
        }

        public Vector3 Update(float deltaTime)
        {
            if (!IsActive)
            {
                return currentValue;
            }

            // Frame spike'inda tek adimda buyuk sicrama olmasin
            deltaTime = Mathf.Min(deltaTime, MAX_DELTA_TIME);

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
                    Vector3 C = (a * (v0 + a * x0) / wd) + x0 * wd;

                    currentValue = expTerm * (x0 * cosT + B * sinT);
                    currentVelocity = expTerm * (v0 * cosT - C * sinT);
                }
                else
                {
                    Vector3 k = v0 + a * x0;

                    currentValue = expTerm * (x0 + k * deltaTime);
                    currentVelocity = expTerm * (v0 - a * deltaTime * k);
                }
            }

            if (currentValue.sqrMagnitude < POS_EPS_SQR && currentVelocity.sqrMagnitude < VEL_EPS_SQR)
            {
                currentValue = Vector3.zero;
                currentVelocity = Vector3.zero;
                IsActive = false;
            }

            return currentValue;
        }
    }
}