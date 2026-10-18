using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public sealed class EnvironmentSettings
    {
        [Header("_")]
        [Info("R -> Mask, G -> Distortion (X), B -> Distortion (Y)")]
        public Texture2D CloudTexture;

        [Header("_")]
        public FogMode FogMode;
        public FogResolution FogResolution;
        public FogQuality FogQuality;
        [Info("R -> Mask")]
        public Texture2D FogTexture;
        [Range(0, 1)] public float FogBlur;

        [Header("_")]
        public SkySettings Sky;
        public StarSettings Stars;
        public CelestialSettings Sun;
        public CelestialSettings Moon;
        public FogSettings Fog;
        public CloudSettings Cloud;
        public AmbientSettings Ambient;

        [HideInInspector] public float Blend;
        [HideInInspector] public Texture2D CloudTextureB;

        internal EnvironmentSettings()
        {
            CloudTexture = default;
            FogMode = FogMode.DEFAULT;
            FogQuality = FogQuality.HIGH;
            FogResolution = FogResolution.HALF;
            FogTexture = default;
            FogBlur = 0;

            Sky = new()
            {
                ZenithColor = Color.aliceBlue,
                HorizonColor = Color.whiteSmoke,
                HorizonSmoothness = 0.5f,
                HorizonThickness = 0.0f,
            };

            Stars = new()
            {
                StarColor = Color.wheat,
                StarAmount = 0,
                StarDensity = 256,
                StarSize = 0.05f,
                StarBrightness = 0.1f,
                StarTwinkleAmount = 0.1f,
                StarTwinkleSpeed = 0.025f,
            };

            Sun = new()
            {
                Color = new(1, 0.75f, 0.25f),
                Intensity = 1,
                Power = 5,
                Size = 0.001f,
                Glow = 1024,
            };

            Moon = new()
            {
                Color = Color.whiteSmoke,
                Intensity = 1,
                Power = 5,
                Size = 0.001f,
                Glow = 1024,
            };

            Fog = new()
            {
                Color = Color.gray,
                Density = 0f,
                Opacity = 1f,
                DistanceStart = 512f,
                DistanceFalloff = 1,
                HeightStart = 10000f,
                HeightFalloff = 4f,
                Scattering = 4f,
                NoiseOpacity = 1f,
                NoiseModulation = 0.25f,
                NoiseOffset = 0.15f,
                NoiseScale = 0.5f,
                NoiseSpeed = 0.5f,
            };

            Cloud = new()
            {
                Tint = Color.white,
                Coverage = 1,
                Opacity = 1,
                Fade = 1,
                Height = 8f,
                Curve = 128,
                Scale = 0.15f,
                Speed = 0.5f,
                Turbulance = 0.1f,
                Transmittance = 0.5f,
                Darkness = 0.5f,
                RimWidth = 0.035f,
                RimStrength = 5f,
            };

            Ambient = new()
            {
                SkyColor = Color.aliceBlue,
                EquatorColor = Color.whiteSmoke,
                GroundColor = Color.gray,
            };
        }
    }
}
