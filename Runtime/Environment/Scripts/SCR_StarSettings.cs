using System;
using UnityEngine;

namespace Core.Environment
{
    [Serializable]
    public struct StarSettings
    {
        [ColorUsage(false, true)] public Color StarColor;
        [Range(0, 1)] public float StarAmount;
        [Min(0)] public float StarDensity;
        [Range(0, 1)] public float StarSize;
        [Min(0)] public float StarBrightness;
        [Range(0, 1)] public float StarTwinkleAmount;
        [Min(0)] public float StarTwinkleSpeed;

        public static StarSettings Lerp(StarSettings a, StarSettings b, float t)
        {
            return new()
            {
                StarColor = Color.Lerp(a.StarColor, b.StarColor, t),
                StarAmount = Mathf.Lerp(a.StarAmount, b.StarAmount, t),
                StarDensity = Mathf.Lerp(a.StarDensity, b.StarDensity, t),
                StarSize = Mathf.Lerp(a.StarSize, b.StarSize, t),
                StarBrightness = Mathf.Lerp(a.StarBrightness, b.StarBrightness, t),
                StarTwinkleAmount = Mathf.Lerp(a.StarTwinkleAmount, b.StarTwinkleAmount, t),
                StarTwinkleSpeed = Mathf.Lerp(a.StarTwinkleSpeed, b.StarTwinkleSpeed, t),
            };
        }
    }
}