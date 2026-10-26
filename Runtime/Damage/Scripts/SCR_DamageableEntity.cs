using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    [DisallowMultipleComponent]
    public sealed class DamageableEntity : MonoBehaviour
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

            Bind(new Damageable(health, protections, ignoredTags.CreateMask(), GetComponents<IDamageableHandler>()));
        }
        private void OnDestroy() => Unbind();

        private void EnsureBind()
        {
            if (damageable == null)
            {
                throw new InvalidOperationException($"{nameof(DamageableEntity)} is not bound to a Damageable.");
            }
        }
        public void Bind(Damageable damageable)
        {
            if (damageable == null)
            {
                throw new ArgumentNullException(nameof(damageable), "Damageable bind failed! damage is null!?");
            }

            Unbind();

            health = damageable.Health;
            this.damageable = damageable;
            this.damageable.OnHit += HandleHit;
        }
        private void Unbind()
        {
            if (damageable == null)
            {
                return;
            }

            damageable.OnHit -= HandleHit;
            damageable = null;
        }

        public void AddHandler(IDamageableHandler handler)
        {
            EnsureBind();
            damageable.AddHandler(handler);
        }
        public void RemoveHandler(IDamageableHandler handler)
        {
            EnsureBind();
            damageable.RemoveHandler(handler);
        }

        public IReadOnlyList<DamageProtection> GetProtections()
        {
            EnsureBind();
            return damageable.GetProtections();
        }
        public void AddProtection(DamageProtection protection)
        {
            EnsureBind();
            damageable.AddProtection(protection);
        }
        public void RemoveProtection(int id)
        {
            EnsureBind();
            damageable.RemoveProtection(id);
        }

        public void Damage(in DamageData data, out DamageContext ctx)
        {
            EnsureBind();
            damageable.Damage(in data, out ctx);
        }
        private void HandleHit(DamageContext context) => OnHit?.Invoke(context);

        public DamageContext GetLastContext()
        {
            EnsureBind();
            return damageable.GetLastContext();
        }
    }
}
