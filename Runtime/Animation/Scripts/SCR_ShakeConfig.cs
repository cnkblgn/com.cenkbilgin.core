using System;
using UnityEngine;

namespace Core.Animation
{
    [Serializable]
    public struct ShakeConfig
    {
        public Vector3 Influence;
        [Min(0)] public float Magnitude;
        [Min(0)] public float Roughness;
        [Min(0)] public float FadeInTime;
        [Min(0)] public float FadeOutTime;

        public readonly static ShakeConfig New = new(1, 1, 0, 1, Vector3.zero);
        public ShakeConfig(ShakeConfig config) => (Magnitude, Roughness, FadeInTime, FadeOutTime, Influence) = (config.Magnitude, config.Roughness, config.FadeInTime, config.FadeOutTime, config.Influence);
        public ShakeConfig(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 influence) => (Magnitude, Roughness, FadeInTime, FadeOutTime, Influence) = (magnitude, roughness, fadeInTime, fadeOutTime, influence);
    }
}