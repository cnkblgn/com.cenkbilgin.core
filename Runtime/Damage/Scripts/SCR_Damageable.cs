using System;
using System.Collections.Generic;

namespace Core.Damage
{
    public sealed class Damageable
    {
        public event Action<DamageContext> OnHit;

        public readonly ResourceContainer Health;
        private readonly List<DamageProtection> protections;
        private readonly List<IDamageableHandler> handlers;
        private readonly DamageMask ignoreMask;
        private DamageContext lastContext;

        public Damageable(ResourceContainer health, List<DamageProtection> protections = null, DamageMask ignoreMask = default, List<IDamageableHandler> handlers = null)
        {
            this.Health = health ?? throw new ArgumentNullException(nameof(health), "Damageable health cannot be null!");
            this.protections = protections == null ? new() : new(protections);
            this.ignoreMask = ignoreMask;
            this.handlers = handlers == null ? new() : new(handlers);
        }
        public Damageable(Damageable damageable, List<IDamageableHandler> handlers = null) : this(damageable == null ? throw new ArgumentNullException(nameof(damageable)) : damageable.Health, damageable.protections, damageable.ignoreMask, handlers) { }

        public void AddHandler(IDamageableHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (handlers.Contains(handler))
            {
                return;
            }

            handlers.Add(handler);
        }
        public void RemoveHandler(IDamageableHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!handlers.Contains(handler))
            {
                return;
            }

            handlers.Remove(handler);
        }

        public IReadOnlyList<DamageProtection> GetProtections() => protections;
        public void AddProtection(DamageProtection protection)
        {
            for (int i = 0; i < protections.Count; i++)
            {
                if (protections[i].ID == protection.ID)
                {
                    protections[i] = protection;
                    return;
                }
            }

            protections.Add(protection);
        }
        public void RemoveProtection(int id)
        {
            for (int i = protections.Count - 1; i >= 0; i--)
            {
                if (protections[i].ID == id)
                {
                    protections.RemoveAt(i);
                    return;
                }
            }
        }

        public void Damage(in DamageData data, out DamageContext ctx)
        {
            lastContext = new(data);

            float damage = ResolveDamage(in data);

            lastContext.Damage = damage;

            Health.SetCurrent(Health.GetCurrent() - damage);

            if (!Health.IsDepleted())
            {
                lastContext.State = DamageState.HIT;
            }
            else
            {
                lastContext.State = DamageState.DEATH;
            }

            ctx = lastContext;

            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleHit(in lastContext);
            }

            OnHit?.Invoke(lastContext);
        }
        private float ResolveDamage(in DamageData data)
        {
            float value = data.Damage;

            if (ignoreMask.HasAny(new DamageMask(data.Mask)))
            {
                return 0;
            }

            value = ResolveProtection(value, data.Mask);

            for (int i = 0; i < handlers.Count; i++)
            {
                IDamageableHandler handler = handlers[i];

                if (handler != null)
                {
                    value = handler.HandleDamaged(data, value);
                }
            }

            return value;
        }
        private float ResolveProtection(float value, ulong tags)
        {
            for (int i = 0; i < protections.Count; i++)
            {
                DamageProtection protection = protections[i];

                if (tags.HasAny(protection.Tag.Mask.Value))
                {
                    value -= value * protection.Percent;
                }
            }

            return value;
        }

        public DamageMask GetIgnoreMask() => ignoreMask;
        public DamageContext GetLastContext() => lastContext;
    }
}
