using UnityEngine;
using Core.Damage;

namespace Core.Projectile
{
    using static HitUtility;

    public static class ProjectileUtility
    {
        private readonly static RaycastHit[] tempDirectBuffer = new RaycastHit[4];
        private readonly static HitData[] tempDirectResult = new HitData[4];

        public static bool TryCreateEntity(Transform owner, Projectile prefab, Vector3 position, Quaternion rotation, Vector3 velocity, Collider[] areaOverlapBuffer, RaycastHit[] areaObstructionBuffer, HitData[] areaResultBuffer, int hittableMask, int damageableMask, uint damageContext, in DamageSettings damageSettings, ProjectileSettings projectileSettings, out Projectile entity, IDamageProcessor damageProcessor = null, IHitListener hitListener = null)
        {
            entity = null;

            if (prefab == null)
            {
                Debug.LogError("Projectile creation failed! projectile prefab missing !?");
                return false;
            }

            entity = GameObject.Instantiate(prefab, position, rotation);
            entity.Initialize(owner, velocity, hittableMask, areaOverlapBuffer, areaObstructionBuffer, areaResultBuffer, damageableMask, damageContext, damageSettings, projectileSettings, damageProcessor, hitListener);

            return true;
        }       
        public static bool TryCreateHitScanDirect(Transform owner, Vector3 origin, Vector3 direction, float range, int hittableMask, int damageableMask, uint damageContext, in DamageSettings damageSettings, IDamageProcessor damageProcessor, IHitListener hitListener)
        {
            if (!HitScanClosest(origin, direction, damageSettings.MaxForce, range, hittableMask, tempDirectBuffer, tempDirectResult, QueryTriggerInteraction.Ignore, out HitData result, hitListener))
            {
                return false;
            }

            return DamageUtility.TryDamageDirect(owner, result.Collider, result.Point, result.Normal, direction, damageableMask, damageContext, in damageSettings, damageProcessor);
        }
        public static bool TryCreateHitScanArea(Transform owner, Vector3 origin, Vector3 direction, float range, Collider[] overlapBuffer, RaycastHit[] obstructionBuffer, HitData[] resultBuffer, int hittableMask, int damageableMask, uint damageContext, in DamageSettings damageSettings, IDamageProcessor damageProcessor, IHitListener hitListener)
        {
            if (!HitScanClosest(origin, direction, damageSettings.MaxForce, range, hittableMask, tempDirectBuffer, tempDirectResult, QueryTriggerInteraction.Ignore, out HitData result, hitListener))
            {
                return false;
            }

            if (!HitArea(result.Point, damageSettings.GetRandomForce(), damageSettings.Radius, damageableMask, hittableMask, overlapBuffer, obstructionBuffer, resultBuffer, QueryTriggerInteraction.Collide, out int resultCount))
            {
                return false;
            }

            return DamageUtility.TryDamageArea(owner, resultBuffer, resultCount, result.Point, damageableMask, damageContext, in damageSettings, damageProcessor);
        }
    }
}