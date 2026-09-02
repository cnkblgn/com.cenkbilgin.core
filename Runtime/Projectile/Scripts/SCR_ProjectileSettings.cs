using System;
using UnityEngine;

namespace Core.Projectile
{
    [Serializable]
    public struct ProjectileSettings
    {
        public static ProjectileSettings Default => new(10, true, true, true, false);

        [Min(1)] public float Life;
        public bool Gravity;
        public bool DestroyOnImpact;
        public bool ExecuteOnImpact;
        public bool SnapOnImpact;

        public ProjectileSettings(float life, bool gravity, bool destroyOnImpact, bool executeOnImpact, bool snapOnImpact)
        {
            Life = life;
            Gravity = gravity;
            DestroyOnImpact = destroyOnImpact;
            ExecuteOnImpact = executeOnImpact;
            SnapOnImpact = snapOnImpact;
        }
    }
}