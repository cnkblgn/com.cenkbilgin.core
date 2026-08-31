using System;
using UnityEngine;

namespace Core.Damage
{
    [Serializable]
    public class DamageSettings
    {
        public DamageMode Mode = DamageMode.DIRECT;
        public DamageTag[] Tags = default;
        [Min(0)] public float MinDamage = 1;
        [Min(0)] public float MaxDamage = 1;
        [Min(0)] public float MinForce = 1;
        [Min(0)] public float MaxForce = 1;
        [Min(0)] public float Radius = 1;

        public float GetDamage(float multiplier = 1) => UnityEngine.Random.Range(MinDamage, MaxDamage) * multiplier;
        public float GetForce(float multiplier = 1) => UnityEngine.Random.Range(MinForce, MaxForce) * multiplier;
    }
}
