using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public sealed class EnvironmentSettings
    {
        internal static readonly EnvironmentSettings Default = new();

        [Header("_")]
        [Info("R -> Mask, G -> Distortion (X), B -> Distortion (Y)")]
        public Texture2D CloudTexture;
        [Info("R -> Mask")]
        public Texture2D FogTexture;

        [Header("_")]
        public SkySettings Sky;
        public AmbientSettings Ambient;
        public CelestialSettings Sun;
        public CelestialSettings Moon;
        public FogSettings Fog;
        public CloudSettings Cloud;

        [HideInInspector] public float Blend;
        [HideInInspector] public Texture2D CloudTextureB;

        internal EnvironmentSettings()
        {
            CloudTexture = default;
            FogTexture = default;

            Sky = new()
            {
                ZenithColor = Color.aliceBlue,
                HorizonColor = Color.whiteSmoke,
                HorizonSmoothness = 0.5f,
                HorizonThickness = 0.0f,
            };

            Ambient = new()
            {
                SkyColor = Color.aliceBlue,
                EquatorColor = Color.whiteSmoke,
                GroundColor = Color.gray,
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
                Density = 1f,
                DistanceStart = 512f,
                DistanceFalloff = 1,
                HeightStart = 10000f,
                HeightFalloff = 4f,
                Scattering = 4f,
                NoiseStrength = 0.25f,
                NoiseScale = 0.05f,
                NoiseTurbulance = 0.5f,
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
        }
    }
}
