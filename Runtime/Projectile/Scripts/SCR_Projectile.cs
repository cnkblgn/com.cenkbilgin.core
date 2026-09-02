using UnityEngine;
using Core.Damage;

namespace Core.Projectile
{
    using static HitUtility;

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class Projectile : MonoBehaviour
    {
        private Rigidbody thisRigidbody = null;
        private Collider thisCollider = null;
        private Transform thisTransform = null;
        private Transform ownerTransform = null;
        private ProjectileSettings projectileSettings = default;
        private DamageSettings damageSettings = default;
        private IDamageProcessor damageProcessor = null;
        private IHitListener hitListener = null;
        private Collider[] areaOverlapBuffer = null;
        private RaycastHit[] areaObstructionBuffer = null;
        private HitData[] areaResultBuffer = null;
        private float timer = 0;
        private uint damageContext = 0;
        private int damageableMask = 0;
        private int hittableMask = 0;
        private bool isInitialized = false;
        private bool isExecuted = false;
        private bool isSnapped = false;

        private void FixedUpdate()
        {
            if (!isInitialized || isSnapped)
            {
                return;
            }

            timer += Time.fixedDeltaTime;

            if (timer >= projectileSettings.Life)
            {
                if (!isExecuted && projectileSettings.ExecuteOnImpact)
                {
                    Execute(null, thisTransform.position, thisTransform.forward);
                }

                Destroy(gameObject);
                return;
            }

            if (thisRigidbody.linearVelocity.sqrMagnitude > 0.001f)
            {
                thisTransform.rotation = Quaternion.LookRotation(thisRigidbody.linearVelocity);
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (!isInitialized || isExecuted || isSnapped)
            {
                return;
            }

            if (collision.collider.isTrigger)
            {
                return;
            }

            ContactPoint contact = collision.GetContact(0);

            if (projectileSettings.ExecuteOnImpact)
            {
                Execute(collision.collider, contact.point, contact.normal);
            }

            isExecuted = true;

            if (projectileSettings.SnapOnImpact)
            {
                isSnapped = true;
                thisRigidbody.isKinematic = true;
                thisCollider.enabled = false;
                thisTransform.position = contact.point + thisTransform.forward * 0.02f;
                thisTransform.SetParent(collision.transform, true);
                return;
            }

            if (projectileSettings.DestroyOnImpact)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(Transform owner, Vector3 velocity, int hittableMask, Collider[] areaOverlapBuffer, RaycastHit[] areaObstructionBuffer, HitData[] areaResultBuffer, int damageableMask, uint damageContext, DamageSettings damageSettings, ProjectileSettings projectileSettings, IDamageProcessor damageProcessor = null, IHitListener hitListener = null)
        {
            this.hittableMask = hittableMask;
            this.projectileSettings = projectileSettings;
            this.areaOverlapBuffer = areaOverlapBuffer;
            this.areaObstructionBuffer = areaObstructionBuffer;
            this.areaResultBuffer = areaResultBuffer;
            this.damageableMask = damageableMask;
            this.damageSettings = damageSettings;
            this.damageProcessor = damageProcessor;
            this.damageContext = damageContext;
            this.hitListener = hitListener;

            ownerTransform = owner;
            thisTransform = GetComponent<Transform>();
            thisRigidbody = GetComponent<Rigidbody>();
            thisCollider = GetComponent<Collider>();

            thisRigidbody.linearVelocity = velocity;
            thisRigidbody.useGravity = projectileSettings.Gravity;
            thisRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            thisRigidbody.includeLayers = hittableMask;
            thisRigidbody.excludeLayers = ~hittableMask;

            timer = 0f;
            isInitialized = true;
        }
        private void Execute(Collider collider, Vector3 position, Vector3 normal)
        {
            switch (damageSettings.Mode)
            {
                case DamageMode.DIRECT:
                    DamageUtility.TryDamageDirect(ownerTransform, collider, position, normal, -normal, damageableMask, damageContext, in damageSettings, damageProcessor);
                    break;
                case DamageMode.AREA:
                    if (HitArea(position, damageSettings.GetRandomForce(), damageSettings.Radius, damageableMask, hittableMask, areaOverlapBuffer, areaObstructionBuffer, areaResultBuffer, QueryTriggerInteraction.Collide, out int resultCount))
                    {
                        DamageUtility.TryDamageArea(ownerTransform, areaResultBuffer, resultCount, position, damageableMask, damageContext, damageSettings, damageProcessor);
                    }
                    break;
            }

            hitListener?.HandleHit(new(collider, position, normal, 0.1f, damageSettings.GetRandomForce()));
        }
    }
}
