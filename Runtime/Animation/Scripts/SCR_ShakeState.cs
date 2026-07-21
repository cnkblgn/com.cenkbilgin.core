using UnityEngine;

namespace Core.Animation
{
    public struct ShakeState
    {
        public bool IsActive { get; private set; }

        private Vector3 influence;
        private Vector3 currentValue;
        private float magnitude;
        private float roughness;
        private float fadeInTime;
        private float fadeOutTime;
        private float fadeTimer;
        private float tickTimer;
        private bool isSustaining;

        public void Start(ShakeConfig config, float strength)
        {
            magnitude = config.Magnitude * strength;
            roughness = config.Roughness;
            fadeInTime = config.FadeInTime;
            fadeOutTime = Mathf.Max(0.0001f, config.FadeOutTime);
            influence = config.Influence;

            fadeTimer = fadeInTime > 0 ? 0f : 1f;
            tickTimer = Random.value * 1000f;

            isSustaining = fadeInTime > 0;
            IsActive = true;
        }
        public Vector3 Update(float deltaTime)
        {
            if (!IsActive)
            {
                return currentValue;
            }

            // Noise
            float nx = Mathf.PerlinNoise(tickTimer, 0f) - 0.5f;
            float ny = Mathf.PerlinNoise(0f, tickTimer) - 0.5f;
            float nz = Mathf.PerlinNoise(tickTimer, tickTimer) - 0.5f;

            // Fade in / out
            if (isSustaining)
            {
                if (fadeInTime > 0)
                {
                    fadeTimer += deltaTime / fadeInTime;
                    if (fadeTimer >= 1f)
                    {
                        fadeTimer = 1f;
                        isSustaining = false;
                    }
                }
            }
            else
            {
                fadeTimer -= deltaTime / fadeOutTime;
            }

            if (fadeTimer <= 0f)
            {
                IsActive = false;
                currentValue = Vector3.zero;
                return currentValue;
            }

            tickTimer += deltaTime * roughness * fadeTimer;

            currentValue.x = nx * magnitude * fadeTimer * influence.x;
            currentValue.y = ny * magnitude * fadeTimer * influence.y;
            currentValue.z = nz * magnitude * fadeTimer * influence.z;

            return currentValue;
        }
    }
}