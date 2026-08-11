using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct CelestialSettings
    {
        public Color Color;
        [Min(0)] public float Intensity;
        [Min(0)] public float Power;
        [Min(0)] public float Size;
        [Min(1)] public float Glow;

        public static CelestialSettings Lerp(CelestialSettings a, CelestialSettings b, float t)
        {
            return new()
            {
                Color = Color.Lerp(a.Color, b.Color, t),
                Intensity = Mathf.Lerp(a.Intensity, b.Intensity, t),
                Power = Mathf.Lerp(a.Power, b.Power, t),
                Size = Mathf.Lerp(a.Size, b.Size, t),
                Glow = Mathf.Lerp(a.Glow, b.Glow, t),
            };
        }
    }
}
