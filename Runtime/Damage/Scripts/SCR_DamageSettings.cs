using System;

namespace Core.Damage
{
    [Serializable]
    public class DamageSettings
    {
        public DamageMode Mode = DamageMode.DIRECT;
        public DamageTag[] Tags = default;
        public float MinDamage = 1;
        public float MaxDamage = 1;
        public float MinForce = 1;
        public float MaxForce = 1;
        public float Radius = 1;

        public float GetDamage(float multiplier = 1) => UnityEngine.Random.Range(MinDamage, MaxDamage) * multiplier;
        public float GetForce(float multiplier = 1) => UnityEngine.Random.Range(MinForce, MaxForce) * multiplier;
    }
}
