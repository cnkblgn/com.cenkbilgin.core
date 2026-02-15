using System;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsBody : MonoBehaviour
    {
        public event Action<Collision> OnCollision = null;

        public Transform ThisTransform => thisTransform;
        public Rigidbody ThisRigidbody => thisRigidbody;

        [Header("_")]
        [SerializeField] private bool isKinematicOnStart = false;
        [SerializeField] private LayerMask collisionMask = 0;
        [SerializeField, Min(0.01f)] private float collisionCooldown = 0.15f;

        private Collider[] thisColliders = null;
        private Transform thisTransform = null;
        private Rigidbody thisRigidbody = null;
        private float lastCollisionTime = -Mathf.Infinity;

        private void Awake()
        {
            thisTransform = GetComponent<Transform>();
            thisRigidbody = GetComponent<Rigidbody>();
            thisColliders = GetComponentsInChildren<Collider>();

            foreach (Collider collider in thisColliders)
            {
                collider.includeLayers = collisionMask;
                collider.excludeLayers = ~collisionMask;
            }

            thisRigidbody.includeLayers = collisionMask;
            thisRigidbody.excludeLayers = ~collisionMask;

            if (isKinematicOnStart)
            {
                DisablePhysics();
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (!thisRigidbody.useGravity || thisRigidbody.isKinematic)
            {
                return;
            }

            if (Time.time - lastCollisionTime >= collisionCooldown)
            {
                lastCollisionTime = Time.time;
                OnCollision?.Invoke(collision);
            }
        }

        public void EnablePhysics() => ThisRigidbody.isKinematic = false;
        public void DisablePhysics() => ThisRigidbody.isKinematic = true;

        public void ExcludeLayer(int layer)
        {
            ThisRigidbody.excludeLayers |= (1 << layer);
            ThisRigidbody.includeLayers &= ~(1 << layer);

            foreach (Collider collider in thisColliders)
            {
                collider.excludeLayers |= (1 << layer);
                collider.includeLayers &= ~(1 << layer);
            }
        }
        public void IncludeLayer(int layer)
        {
            if ((collisionMask & (1 << layer)) == 0)
            {
                return;
            }

            ThisRigidbody.excludeLayers &= ~(1 << layer);
            ThisRigidbody.includeLayers |= (1 << layer);

            foreach (Collider collider in thisColliders)
            {
                collider.excludeLayers &= ~(1 << layer);
                collider.includeLayers |= (1 << layer);
            }
        }

        public void AddForce(Vector3 force, ForceMode mode)
        {
            if (ThisRigidbody.isKinematic)
            {
                EnablePhysics();
            }

            ThisRigidbody.AddForce(force, mode);
        }
        public void AddTorque(Vector3 force, ForceMode mode)
        {
            if (ThisRigidbody.isKinematic)
            {
                EnablePhysics();
            }

            ThisRigidbody.AddTorque(force, mode);
        }
    }
}
