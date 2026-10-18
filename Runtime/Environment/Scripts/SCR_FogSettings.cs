using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct FogSettings
    {
        public Color Color;
        [Range(0, 1)] public float Density;
        [Range(0, 1)] public float Opacity;
        [Min(0)] public float DistanceStart;
        [Min(0)] public float DistanceFalloff;
        [Min(0)] public float HeightStart;
        [Min(0)] public float HeightFalloff;
        [Range(0, 2)] public float Scattering;
        [Range(0, 1)] public float NoiseOpacity;
        [Range(0, 1)] public float NoiseModulation;
        [Range(0, 2)] public float NoiseOffset;
        [Range(0, 1)] public float NoiseSpeed;
        [Range(0, 1)] public float NoiseScale;

        public static FogSettings Lerp(FogSettings a, FogSettings b, float t)
        {
            return new()
            {
                Color = Color.Lerp(a.Color, b.Color, t),
                Density = Mathf.Lerp(a.Density, b.Density, t),
                Opacity = Mathf.Lerp(a.Opacity, b.Opacity, t),
                DistanceStart = Mathf.Lerp(a.DistanceStart, b.DistanceStart, t),
                DistanceFalloff = Mathf.Lerp(a.DistanceFalloff, b.DistanceFalloff, t),
                HeightStart = Mathf.Lerp(a.HeightStart, b.HeightStart, t),
                HeightFalloff = Mathf.Lerp(a.HeightFalloff, b.HeightFalloff, t),
                Scattering = Mathf.Lerp(a.Scattering, b.Scattering, t),
                NoiseOpacity = Mathf.Lerp(a.NoiseOpacity, b.NoiseOpacity, t),
                NoiseModulation = Mathf.Lerp(a.NoiseModulation, b.NoiseModulation, t),
                NoiseOffset = Mathf.Lerp(a.NoiseOffset, b.NoiseOffset, t),
                NoiseSpeed = Mathf.Lerp(a.NoiseSpeed, b.NoiseSpeed, t),
                NoiseScale = Mathf.Lerp(a.NoiseScale, b.NoiseScale, t),
            };
        }
    }
}