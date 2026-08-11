using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct CloudSettings
    {
        public Color Tint;
        [Range(0, 1)] public float Coverage;
        [Range(0, 1)] public float Opacity;
        [Range(0, 1)] public float Fade;
        [Min(0)] public float Height;
        [Min(0)] public float Curve;
        [Range(0, 1)] public float Scale;
        [Min(0)] public float Speed;
        [Min(0)] public float Turbulance;
        [Range(0, 4)] public float Transmittance;
        [Range(0, 1)] public float Darkness;
        [Range(0, 1)] public float RimWidth;
        [Min(0)] public float RimStrength;

        public static CloudSettings Lerp(CloudSettings a, CloudSettings b, float t)
        {
            return new()
            {
                Tint = Color.Lerp(a.Tint, b.Tint, t),
                Coverage = Mathf.Lerp(a.Coverage, b.Coverage, t),
                Opacity = Mathf.Lerp(a.Opacity, b.Opacity, t),
                Fade = Mathf.Lerp(a.Fade, b.Fade, t),
                Height = Mathf.Lerp(a.Height, b.Height, t),
                Curve = Mathf.Lerp(a.Curve, b.Curve, t),
                Scale = Mathf.Lerp(a.Scale, b.Scale, t),
                Speed = Mathf.Lerp(a.Speed, b.Speed, t),
                Turbulance = Mathf.Lerp(a.Turbulance, b.Turbulance, t),
                Transmittance = Mathf.Lerp(a.Transmittance, b.Transmittance, t),
                Darkness = Mathf.Lerp(a.Darkness, b.Darkness, t),
                RimWidth = Mathf.Lerp(a.RimWidth, b.RimWidth, t),
                RimStrength = Mathf.Lerp(a.RimStrength, b.RimStrength, t),
            };
        }
    }
}