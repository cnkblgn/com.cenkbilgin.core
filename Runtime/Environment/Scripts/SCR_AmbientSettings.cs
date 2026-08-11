using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct AmbientSettings
    {
        [ColorUsage(false, true)] public Color SkyColor;
        [ColorUsage(false, true)] public Color EquatorColor;
        [ColorUsage(false, true)] public Color GroundColor;

        public static AmbientSettings Lerp(AmbientSettings a, AmbientSettings b, float t)
        {
            return new()
            {
                SkyColor = Color.Lerp(a.SkyColor, b.SkyColor, t),
                EquatorColor = Color.Lerp(a.EquatorColor, b.EquatorColor, t),
                GroundColor = Color.Lerp(a.GroundColor, b.GroundColor, t),
            };
        }
    }
}
