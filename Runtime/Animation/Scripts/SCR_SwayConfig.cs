using System;
using UnityEngine;
using Unity.Mathematics;

namespace Core.Animation
{
    [Serializable]
    public class SwayConfig
    {
        [Min(0.01f)] public float Smoothness;
        public Vector3 Amplitude;
        public Vector3 Clamp;
        public bool3 IsOffset;

        public static SwayConfig New => new(0.15f, Vector3.one, Vector3.zero, new(false, false, false));
        public SwayConfig(float smoothness, Vector3 amplitude, Vector3 clamp, bool3 offset) => (Smoothness, Amplitude, Clamp, IsOffset) = (smoothness, amplitude, clamp, offset);
    }
}