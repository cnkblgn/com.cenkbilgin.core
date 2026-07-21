using System;
using UnityEngine;

namespace Core.Animation
{
    [Serializable]
    public class PoseConfig
    {
        [Min(0.01f)] public float Smoothness;
        public Vector3 Target;

        public static PoseConfig New => new(5, Vector3.zero);
        public PoseConfig(float smoothness, Vector3 target) => (Smoothness, Target) = (smoothness, target);
    }
}