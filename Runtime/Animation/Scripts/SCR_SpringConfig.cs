using System;
using UnityEngine;

namespace Core.Animation
{
    [Serializable]
    public struct SpringConfig
    {
        public Vector3 Amplitude;
        [Min(0)] public float Frequency;
        [Range(0, 1)] public float Damping;

        public readonly static SpringConfig New = new(Vector3.zero, 8, 0.33f);
        public SpringConfig(Vector3 amplitude, float frequency, float damping) => (Amplitude, Frequency, Damping) = (amplitude, Mathf.Max(0, frequency), Mathf.Clamp01(damping));
        public SpringConfig(SpringConfig config) => (Amplitude, Frequency, Damping) = (config.Amplitude, Mathf.Max(0, config.Frequency), Mathf.Clamp01(config.Damping));
    }
}