using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    [DisallowMultipleComponent]
    public sealed class DamageableEntity : MonoBehaviour, IDamageableHandler
    {
        public event Action<DamageContext> OnHit;

        public ResourceContainer Health => damageable.Health;
        public Vector3 Position => origin.position;
        public Quaternion Rotation => origin.rotation;

        [Header("_")]
        [SerializeField] private ResourceContainer health = new(100, 100);

        [Header("_")]
        [SerializeField, Required] private Transform origin;

        [Header("_")]
        [SerializeField] private DamageTag[] ignoredTags;
        [SerializeField] private List<DamageProtection> protections;

        private Damageable damageable;

        private void Awake()
        {
            if (origin == null) throw new NullReferenceException(nameof(origin));

            IDamageableHandler[] handlers = GetComponents<IDamageableHandler>();

            damageable = new Damageable(health, protections, ignoredTags.CreateMask());

            for (int i = 0; i < handlers.Length; i++)
            {
                damageable.AddHandler(handlers[i]);
            }
        }
        private void OnDestroy() => Unbind();

        public void Bind(Damageable damageable)
        {
            if (damageable == null)
            {
                throw new ArgumentNullException(nameof(damageable), "Damageable bind failed! damage is null!?");
            }

            Unbind();

            health = damageable.Health;
            this.damageable = damageable;
            this.damageable.AddHandler(this);
        }
        private void Unbind()
        {
            if (damageable == null)
            {
                return;
            }

            damageable.RemoveHandler(this);
            damageable = null;
        }

        public IReadOnlyList<DamageProtection> GetProtections() => damageable.GetProtections();
        public void AddProtection(DamageProtection protection) => damageable.AddProtection(protection);
        public void RemoveProtection(int id) => damageable.RemoveProtection(id);

        public void Damage(in DamageData data, out DamageContext ctx) => damageable.Damage(in data, out ctx);

        public float HandleDamaged(in DamageData data, float damage) => damage;
        public void HandleHit(in DamageContext ctx) => OnHit?.Invoke(ctx);

        public DamageContext GetLastContext() => damageable.GetLastContext();
        public DamageMask GetIgnoreMask() => damageable.GetIgnoreMask();
    }
}
