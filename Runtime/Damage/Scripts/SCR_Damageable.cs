using System;
using UnityEngine;

namespace Core.Damage
{
    [DisallowMultipleComponent]
    public sealed class Damageable : MonoBehaviour
    {
        public event Action<DamageContext> OnHit = null;

        public Resource Health => health;
        public Vector3 Position => origin.position;
        public Quaternion Rotation => origin.rotation;

        [Header("_")]
        [SerializeField] private Resource health = new(100, 100);

        [Header("_")]
        [SerializeField, Required] private Transform origin;
        [SerializeField] private DamageTag[] ignoreTags;

        private IDamageableHandler thisHandler;
        private DamageContext lastContext;
        private ulong ignoredTagMask;

        private void Awake()
        {
            if (origin == null) throw new NullReferenceException(nameof(origin));

            thisHandler = GetComponent<IDamageableHandler>();
            ignoredTagMask = ignoreTags.CreateMask();
        }

        public void Bind(Resource health) => this.health = health ?? throw new ArgumentNullException(nameof(health), "You are trying to bind null health resource!? please assign valid resource!");

        public void Damage(in DamageData data, out DamageContext ctx)
        {
            lastContext = new(data);

            float damage = ResolveDamage(in data);

            lastContext.Damage = damage;

            health.SetCurrent(health.GetCurrent() - damage);

            if (!health.IsDepleted())
            {
                lastContext.State = DamageState.HIT;
            }
            else
            {
                lastContext.State = DamageState.DEATH;
            }

            ctx = lastContext;

            thisHandler?.HandleHit(in lastContext);
            OnHit?.Invoke(lastContext);
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
                value = thisHandler.HandleDamaged(data);
            }

            return value;
        }

        public DamageContext GetLastContext() => lastContext;

        public DamageTag[] GetIgnoredTags() => ignoreTags;
        public void SetIgnoredTags(DamageTag[] tags)
        {
            ignoreTags = tags;

            ignoredTagMask = ignoreTags.CreateMask();
        }
    }
}
