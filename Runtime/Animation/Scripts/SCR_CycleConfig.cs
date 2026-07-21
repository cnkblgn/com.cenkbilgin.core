using System;
using UnityEngine;

namespace Core.Animation
{
    [Serializable]
    public sealed class CycleConfig
    {
        [Min(0.01f)] public float Roughness;
        public Vector3 Amplitude;
        public Vector3 Frequency;
        public Vector3 Clamp;

        public static CycleConfig New => new(5, Vector3.one * 0.1f, Vector3.one * 15.0f, Vector3.zero);
        public CycleConfig(float roughness, Vector3 amplitude, Vector3 frequency, Vector3 clamp) => (Roughness, Amplitude, Frequency, Clamp) = (roughness, amplitude, frequency, clamp);
    }
}