using UnityEngine;

namespace Core.Animation
{
    public struct ShakeState
    {
        private const float BLEND_DURATION = 0.08f; // ~80ms yumuþak geçiþ

        public bool IsActive { get; private set; }

        private Vector3 influence;
        private Vector3 currentValue;
        private Vector3 blendValue;
        private float magnitude;
        private float roughness;
        private float fadeInTime;
        private float fadeOutTime;
        private float fadeTimer;
        private float tickTimer;
        private float blendTimer;
        private bool isSustaining;

        public void Start(ShakeConfig config, float strength)
        {
            // Eðer zaten aktifse, mevcut deðeri "eski" olarak sakla ve blend baþlat
            if (IsActive)
            {
                blendValue = currentValue;
                blendTimer = BLEND_DURATION;
            }
            else
            {
                blendValue = Vector3.zero;
                blendTimer = 0f;
            }

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

            Vector3 target;
            target.x = nx * magnitude * fadeTimer * influence.x;
            target.y = ny * magnitude * fadeTimer * influence.y;
            target.z = nz * magnitude * fadeTimer * influence.z;

            if (blendTimer > 0f)
            {
                float t = 1f - (blendTimer / BLEND_DURATION);
                currentValue = Vector3.Lerp(blendValue, target, t);
                blendTimer -= deltaTime;
            }
            else
            {
                currentValue = target;
            }

            return currentValue;
        }
    }
}