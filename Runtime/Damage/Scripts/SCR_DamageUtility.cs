using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    using static CoreUtility;

    public static class DamageUtility
    {
        public static ulong CreateMask(this DamageTag[] tags) => DamageTag.CreateMask(tags);

        public static bool HasAll(this ulong @base, DamageTag target) => @base.HasAll(target.Mask);
        public static bool HasAny(this ulong @base, DamageTag target) => @base.HasAny(target.Mask);

        public static bool HasAll(this DamageTag[] @base, DamageTag[] target) => CreateMask(@base).HasAll(CreateMask(target));
        public static bool HasAny(this DamageTag[] @base, DamageTag[] target) => CreateMask(@base).HasAny(CreateMask(target));

        public static bool TryDamageDirect(Transform owner, Collider collider, Vector3 point, Vector3 normal, Vector3 direction, int damageableMask, ulong tags, uint context, float radius, float damage, float force, IDamageListener listener = null, DamageProcessor processor = null)
        {
            if (collider == null)
            {
                Debug.LogWarning("DamageUtility.TryDamageDirect() collider == null");
                return false;
            }

            Damageable entity = collider.GetComponentInParent<Damageable>();

            if (entity == null)
            {
                return false;
            }

            if (!entity.gameObject.IsInBitMask(damageableMask))
            {
                return false;
            }

            if (processor != null && !processor(tags, collider, direction, damage))
            {
                return false;
            }

            bool hit = entity.TryDamage(new(owner, collider, point, normal, direction, DamageMode.DIRECT, tags, context, radius, damage, force), out DamageContext ctx);

            listener?.HandleDamage(ctx);
            return hit;
        }
        public static bool TryDamageArea(Transform owner, Collider[] colliders, Vector3 point, int damageableMask, ulong tags, uint context, float radius, float minDamage, float maxDamage, float minForce, float maxForce, IDamageListener listener = null, DamageProcessor processor = null)
        {
            if (colliders == null)
            {
                Debug.LogError("damage colliders == null");
                return false;
            }

            if (colliders.Length <= 0)
            {
                Debug.LogWarning("damagecolliders.Length <= 0");
                return false;
            }

            HashSet<Damageable> entities = new(32);
            bool hit = false;

            foreach (Collider collider in colliders)
            {
                if (!collider.gameObject.IsInBitMask(damageableMask))
                {
                    continue;
                }

                Damageable entity = collider.GetComponentInParent<Damageable>();

                if (entity == null || entity.Health.IsDepleted())
                {
                    continue;
                }

                if (!entities.Add(entity))
                {
                    continue;
                }

                Vector3 direction = entity.Origin.position - point;
                float t = Mathf.Clamp01(direction.sqrMagnitude / (radius * radius));
                float damage = Mathf.Lerp(maxDamage, minDamage, t);
                float force = Mathf.Lerp(maxForce, minForce, t);

                direction = direction.normalized;

                if (processor != null && !processor(tags, collider, direction, damage))
                {
                    continue;
                }

                hit = entity.TryDamage(new(owner, collider, point, Vector3.up, direction, DamageMode.AREA, tags, context, radius, damage, force), out DamageContext ctx);

                listener?.HandleDamage(ctx);
            }

            return hit;
        }
        public static bool TryDamageArea(Transform owner, HitData[] results, Vector3 point, int damageableMask, ulong tags, uint context, float radius, float minDamage, float maxDamage, float minForce, float maxForce, IDamageListener listener, DamageProcessor processor = null)
        {
            if (results == null)
            {
                Debug.LogError("damage results == null");
                return false;
            }

            if (results.Length <= 0)
            {
                Debug.LogWarning("damage results.Length <= 0");
                return false;
            }

            HashSet<Damageable> entities = new(32);
            bool hit = false;

            foreach (HitData result in results)
            {
                if (!result.Collider.gameObject.IsInBitMask(damageableMask))
                {
                    continue;
                }

                Damageable entity = result.Collider.GetComponentInParent<Damageable>();

                if (entity == null || entity.Health.IsDepleted())
                {
                    continue;
                }

                if (!entities.Add(entity))
                {
                    continue;
                }

                Vector3 direction = entity.Origin.position - point;

                float t = Mathf.Clamp01(direction.sqrMagnitude / (radius * radius));
                float damage = Mathf.Lerp(maxDamage, minDamage, t);
                float force = Mathf.Lerp(maxForce, minForce, t);

                direction = direction.normalized;

                if (processor != null && !processor(tags, result.Collider, direction, damage))
                {
                    continue;
                }

                hit = entity.TryDamage(new(owner, result.Collider, point, Vector3.up, direction, DamageMode.AREA, tags, context, radius, damage, force), out DamageContext ctx);

                listener?.HandleDamage(ctx);
            }

            return hit;
        }
    }
}