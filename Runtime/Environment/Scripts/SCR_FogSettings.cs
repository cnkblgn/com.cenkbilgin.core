using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct FogSettings
    {
        public Color Color;
        [Range(0, 1)] public float Density;
        [Min(0)] public float DistanceStart;
        [Min(0)] public float DistanceFalloff;
        [Min(0)] public float HeightStart;
        [Min(0)] public float HeightFalloff;
        [Min(0)] public float Scattering;
        [Range(0, 1)] public float NoiseStrength;
        [Range(0, 1)] public float NoiseTurbulance;
        [Range(0, 1)] public float NoiseScale;

        public static FogSettings Lerp(FogSettings a, FogSettings b, float t)
        {
            return new()
            {
                Color = Color.Lerp(a.Color, b.Color, t),
                Density = Mathf.Lerp(a.Density, b.Density, t),
                DistanceStart = Mathf.Lerp(a.DistanceStart, b.DistanceStart, t),
                DistanceFalloff = Mathf.Lerp(a.DistanceFalloff, b.DistanceFalloff, t),
                HeightStart = Mathf.Lerp(a.HeightStart, b.HeightStart, t),
                HeightFalloff = Mathf.Lerp(a.HeightFalloff, b.HeightFalloff, t),
                Scattering = Mathf.Lerp(a.Scattering, b.Scattering, t),
                NoiseStrength = Mathf.Lerp(a.NoiseStrength, b.NoiseStrength, t),
                NoiseTurbulance = Mathf.Lerp(a.NoiseTurbulance, b.NoiseTurbulance, t),
                NoiseScale = Mathf.Lerp(a.NoiseScale, b.NoiseScale, t),
            };
        }
    }
}