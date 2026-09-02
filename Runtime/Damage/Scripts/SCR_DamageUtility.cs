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

        public static bool TryDamageDirect(Transform owner, Collider collider, Vector3 point, Vector3 normal, Vector3 direction, int damageableMask, uint context, in DamageSettings settings, IDamageProcessor processor = null) => TryDamageDirect(owner, collider, point, normal, direction, damageableMask, context, settings.Tags.CreateMask(), settings.Radius, settings.GetRandomDamage(), settings.GetRandomForce(), processor);
        public static bool TryDamageDirect(Transform owner, Collider collider, Vector3 point, Vector3 normal, Vector3 direction, int damageableMask, uint context, in DamageSettings settings, ulong tags, IDamageProcessor processor = null) => TryDamageDirect(owner, collider, point, normal, direction, damageableMask, context, tags, settings.Radius, settings.GetRandomDamage(), settings.GetRandomForce(), processor);
        public static bool TryDamageDirect(Transform owner, Collider collider, Vector3 point, Vector3 normal, Vector3 direction, int damageableMask, uint context, ulong tags, float radius, float damage, float force, IDamageProcessor processor = null)
        {
            if (collider == null)
            {
                Debug.LogWarning("damage area failed! collider == null");
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

            DamageData data = new(owner, collider, point, normal, direction, DamageMode.DIRECT, tags, context, radius, damage, force);

            if (processor != null && !processor.HandleCanDamageTarget(in data))
            {
                return false;
            }

            entity.Damage(in data, out DamageContext ctx);
            processor?.HandleAfterDamagedTarget(ctx);

            return true;
        }
        public static bool TryDamageArea(Transform owner, Collider[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, in DamageSettings settings, IDamageProcessor processor = null) => TryDamageArea(owner, buffer, bufferCount, point, damageableMask, context, settings.Tags.CreateMask(), settings.Radius, settings.MinDamage, settings.MaxDamage, settings.MinForce, settings.MaxForce, processor);
        public static bool TryDamageArea(Transform owner, Collider[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, in DamageSettings settings, ulong tags, IDamageProcessor processor = null) => TryDamageArea(owner, buffer, bufferCount, point, damageableMask, context, tags, settings.Radius, settings.MinDamage, settings.MaxDamage, settings.MinForce, settings.MaxForce, processor);
        public static bool TryDamageArea(Transform owner, Collider[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, ulong tags, float radius, float minDamage, float maxDamage, float minForce, float maxForce, IDamageProcessor processor = null)
        {
            if (buffer == null)
            {
                Debug.LogError("damage area failed! damage collider buffer == null");
                return false;
            }

            if (bufferCount <= 0 || bufferCount > buffer.Length)
            {
                return false;
            }

            HashSet<Damageable> entities = new(32);
            float radiusSqr = radius * radius;
            bool hasHit = false;

            for (int i = 0; i < bufferCount; i++)
            {
                Collider collider = buffer[i];

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
                float factor = Mathf.Clamp01(direction.sqrMagnitude / radiusSqr);
                float totalDamage = Mathf.Lerp(maxDamage, minDamage, factor);
                float totalForce = Mathf.Lerp(maxForce, minForce, factor);
                direction = direction.normalized;

                DamageData data = new(owner, collider, point, Vector3.up, direction, DamageMode.AREA, tags, context, radius, totalDamage, totalForce);

                if (processor != null && !processor.HandleCanDamageTarget(in data))
                {
                    continue;
                }

                entity.Damage(in data, out DamageContext ctx);
                processor?.HandleAfterDamagedTarget(ctx);

                hasHit = true;
            }

            return hasHit;
        }
        public static bool TryDamageArea(Transform owner, HitData[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, in DamageSettings settings, IDamageProcessor processor = null) => TryDamageArea(owner, buffer, bufferCount, point, damageableMask, context, settings.Tags.CreateMask(), settings.Radius, settings.MinDamage, settings.MaxDamage, settings.MinForce, settings.MaxForce, processor);
        public static bool TryDamageArea(Transform owner, HitData[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, in DamageSettings settings, ulong tags, IDamageProcessor processor = null) => TryDamageArea(owner, buffer, bufferCount, point, damageableMask, context, tags, settings.Radius, settings.MinDamage, settings.MaxDamage, settings.MinForce, settings.MaxForce, processor);
        public static bool TryDamageArea(Transform owner, HitData[] buffer, int bufferCount, Vector3 point, int damageableMask, uint context, ulong tags, float radius, float minDamage, float maxDamage, float minForce, float maxForce, IDamageProcessor processor = null)
        {
            if (buffer == null)
            {
                Debug.LogError("damage area failed! damage hit buffer == null");
                return false;
            }

            if (bufferCount <= 0 || bufferCount > buffer.Length)
            {
                return false;
            }

            HashSet<Damageable> entities = new(32);
            float radiusSqr = radius * radius;
            bool hasHit = false;

            for (int i = 0; i < bufferCount; i++)
            {
                HitData result = buffer[i];

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
                float factor = Mathf.Clamp01(direction.sqrMagnitude / radiusSqr);
                float totalDamage = Mathf.Lerp(maxDamage, minDamage, factor);
                float totalForce = Mathf.Lerp(maxForce, minForce, factor);
                direction = direction.normalized;

                DamageData data = new(owner, result.Collider, point, Vector3.up, direction, DamageMode.AREA, tags, context, radius, totalDamage, totalForce);

                if (processor != null && !processor.HandleCanDamageTarget(in data))
                {
                    continue;
                }

                entity.Damage(in data, out DamageContext ctx);
                processor?.HandleAfterDamagedTarget(ctx);

                hasHit = true;
            }

            return hasHit;
        }
    }
}