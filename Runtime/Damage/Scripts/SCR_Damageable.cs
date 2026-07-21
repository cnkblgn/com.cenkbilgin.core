using System;
using UnityEngine;

namespace Core.Damage
{
    [DisallowMultipleComponent]
    public sealed class Damageable : MonoBehaviour
    {
        public event Action<DamageContext> OnHit = null;

        public Transform Origin => origin;
        public Resource Health => health;

        [Header("_")]
        [SerializeField] private Resource health = new(100, 100);

        [Header("_")]
        [SerializeField, Required] private Transform origin = null;
        [SerializeField] private DamageTag[] ignoreTags;

        private IDamageableHandler thisHandler = null;
        private ulong ignoredTagMask;

        private void Awake()
        {
            if (origin == null) throw new NullReferenceException(nameof(origin));

            thisHandler = GetComponent<IDamageableHandler>();
            ignoredTagMask = ignoreTags.CreateMask();
        }

        public void Bind(Resource health) => this.health = health ?? throw new ArgumentNullException(nameof(health));

        public bool TryDamage(in DamageData data, out DamageContext ctx)
        {
            ctx = new(data);

            if (!health.IsDepleted())
            {
                float damage = ResolveDamage(data);
                ctx.Damage = damage;

                health.SetCurrent(health.GetCurrent() - damage);

                if (health.IsDepleted())
                {
                    ctx.State = DamageState.DEATH;
                    thisHandler?.HandleHit(ctx);
                    OnHit?.Invoke(ctx);
                }
            }

            ctx.State = DamageState.HIT;
            thisHandler?.HandleHit(ctx);
            OnHit?.Invoke(ctx);
            return true;
        }
        private float ResolveDamage(in DamageData data)
        {
            float value = data.Damage;

            if (ignoredTagMask.HasAny(data.Tags))
            {
                return 0;
            }

            if (thisHandler != null)
            {
                value = thisHandler.HandleDamage(data);
            }

            return value;
        }

        public DamageTag[] GetIgnoredTags() => ignoreTags;
        public void SetIgnoredTags(DamageTag[] tags)
        {
            ignoreTags = tags;

            ignoredTagMask = ignoreTags.CreateMask();
        }
    }
}
