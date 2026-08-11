using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct SkySettings
    {
        [ColorUsage(false, true)] public Color ZenithColor;
        [ColorUsage(false, true)] public Color HorizonColor;
        [Range(0, 1)] public float HorizonThickness;
        [Range(0, 1)] public float HorizonSmoothness;

        public static SkySettings Lerp(SkySettings a, SkySettings b, float t)
        {
            return new()
            {
                ZenithColor = Color.Lerp(a.ZenithColor, b.ZenithColor, t),
                HorizonColor = Color.Lerp(a.HorizonColor, b.HorizonColor, t),
                HorizonThickness = Mathf.Lerp(a.HorizonThickness, b.HorizonThickness, t),
                HorizonSmoothness = Mathf.Lerp(a.HorizonSmoothness, b.HorizonSmoothness, t),
            };
        }
    }
}