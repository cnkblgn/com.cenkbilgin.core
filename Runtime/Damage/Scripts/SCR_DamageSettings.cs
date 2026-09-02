using System;
using UnityEngine;

namespace Core.Damage
{
    [Serializable]
    public struct DamageSettings
    {
        public static DamageSettings Default => new(DamageMode.DIRECT, default, 1, 1, 1, 1, 1);

        public DamageMode Mode;
        public DamageTag[] Tags;
        [Min(0)] public float MinDamage;
        [Min(0)] public float MaxDamage;
        [Min(0)] public float MinForce;
        [Min(0)] public float MaxForce;
        [Min(0)] public float Radius;

        public DamageSettings(DamageMode mode, DamageTag[] tags, float minDamage, float maxDamage, float minForce, float maxForce, float radius)
        {
            Mode = mode;
            Tags = tags;
            MinDamage = minDamage;
            MaxDamage = maxDamage;
            MinForce = minForce;
            MaxForce = maxForce;
            Radius = radius;
        }

        public readonly float GetRandomDamage(float multiplier = 1) => UnityEngine.Random.Range(MinDamage, MaxDamage) * multiplier;
        public readonly float GetRandomForce(float multiplier = 1) => UnityEngine.Random.Range(MinForce, MaxForce) * multiplier;
    }
}
