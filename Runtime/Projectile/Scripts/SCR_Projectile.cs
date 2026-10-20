using UnityEngine;

namespace Core.Projectile
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class Projectile : MonoBehaviour
    {
        private Rigidbody thisRigidbody;
        private Collider thisCollider;
        private Transform thisTransform;
        private ProjectileSettings projectileSettings;
        private IHitListener hitListener;
        private float timer;
        private bool isInitialized;
        private bool isSnapped;
        private bool hasHit;

        private void FixedUpdate()
        {
            if (!isInitialized)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Projectile [{gameObject}] is not initialized!", gameObject);
#endif
                return;
            }

            if (!isSnapped)
            {
                return;
            }

            timer += Time.fixedDeltaTime;

            if (timer >= projectileSettings.Life)
            {
                if (!hasHit && projectileSettings.ExecuteOnImpact)
                {
                    Hit(null, thisTransform.position, thisTransform.forward);
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
            if (!isInitialized || hasHit || isSnapped)
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
                Hit(collision.collider, contact.point, contact.normal);
            }

            hasHit = true;

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

        public void Initialize(Vector3 velocity, ProjectileSettings settings, int hitMask, IHitListener hitListener = null)
        {
            if (!isInitialized)
            {
                thisTransform = GetComponent<Transform>();
                thisRigidbody = GetComponent<Rigidbody>();
                thisCollider = GetComponent<Collider>();
            }

            this.projectileSettings = settings;
            this.hitListener = hitListener;

            thisRigidbody.linearVelocity = velocity;
            thisRigidbody.useGravity = settings.Gravity;
            thisRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            thisRigidbody.includeLayers = hitMask;
            thisRigidbody.excludeLayers = ~hitMask;

            timer = 0f;
            isInitialized = true;
        }
        private void Hit(Collider collider, Vector3 position, Vector3 normal) => hitListener?.HandleHit(new(collider, position, normal, 0.1f, thisRigidbody.linearVelocity.magnitude));
    }
}
