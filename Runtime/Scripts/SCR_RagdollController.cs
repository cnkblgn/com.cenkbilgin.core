using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class RagdollController : MonoBehaviour
    {
        public event Action OnActivated = null;
        public event Action OnCleared = null;

        [Header("_")]
        [SerializeField] private bool disableCollidersOnAwake = false;

        [Header("_")]
        [SerializeField] private int ragdollLayer = 10;
        [SerializeField] private int armatureLayer = 12;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = -1;

        private Rigidbody[] thisBones = null;
        private Animator[] thisAnimators = null;
        private Collider[] thisColliders = null;
        private Coroutine activationCoroutine = null;
        private Coroutine deactivationCoroutine = null;
        private readonly Dictionary<Collider, Rigidbody> mappedBones = new(2);
        private readonly WaitForSeconds waitTime = new(10);
        private readonly WaitForFixedUpdate waitPhysics = new();
        private bool isDeactivated = false;
        private bool isRagdolled = false;

        private void Awake()
        {
            thisAnimators = GetComponentsInChildren<Animator>();
            thisBones = GetComponentsInChildren<Rigidbody>();
            thisColliders = new Collider[thisBones.Length];

            thisBones[0].gameObject.SetLayer(armatureLayer);
            for (int i = 0; i < thisBones.Length; i++)
            {
                thisColliders[i] = thisBones[i].GetComponent<Collider>();
                thisColliders[i].includeLayers = collisionMask;
                thisColliders[i].excludeLayers = ~collisionMask;
                thisBones[i].isKinematic = true;
                thisBones[i].useGravity = false;
                thisBones[i].collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                mappedBones.Add(thisColliders[i], thisBones[i]);

                if (disableCollidersOnAwake)
                {
                    thisColliders[i].enabled = false;
                }
            }
        }

        public void Hit(Collider collider, Vector3 position, Vector3 direction, float force, float radius, bool isArea)
        {
            if (isDeactivated)
            {
                return;
            }

            if (isArea)
            {
                thisBones[0].AddExplosionForce(force, position, radius, force, ForceMode.Impulse);
                return;
            }

            if (collider == null)
            {
                collider = thisColliders[0];
            }

            if (!mappedBones.TryGetValue(collider, out var rigidbody))
            {
                LogError("RagdollController.Hit() rigidbody == null!", collider);
                return;
            }

            rigidbody.AddForceAtPosition(force * direction, position, ForceMode.Impulse);
        }     
        public void Clear()
        {
            isDeactivated = false;
            isRagdolled = false;

            if (activationCoroutine != null)
            {
                StopCoroutine(activationCoroutine);
                activationCoroutine = null;
            }

            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
                deactivationCoroutine = null;
            }

            for (int i = 0; i < thisAnimators.Length; i++)
            {
                thisAnimators[i].enabled = true;
            }

            for (int i = 0; i < thisBones.Length; i++)
            {
                thisBones[i].angularVelocity = Vector3.zero;
                thisBones[i].linearVelocity = Vector3.zero;
                thisBones[i].isKinematic = true;
                thisBones[i].useGravity = false;
            }

            OnCleared?.Invoke();
        }

        public void Activate(Vector3 velocity)
        {
            if (isDeactivated)
            {
                return;
            }

            if (activationCoroutine != null)
            {
                return;
            }

            OnActivated?.Invoke();

            activationCoroutine = StartCoroutine(ActivateInternal(velocity));
        }       
        private IEnumerator ActivateInternal(Vector3 velocity)
        {
            if (isDeactivated)
            {
                yield break;
            }

            for (int i = 0; i < thisAnimators.Length; i++)
            {
                thisAnimators[i].enabled = false;
            }

            for (int i = 0; i < thisBones.Length; i++)
            {
                thisBones[i].gameObject.SetLayer(ragdollLayer);

                if (thisBones[i].isKinematic)
                {
                    thisBones[i].isKinematic = false;
                }

                thisColliders[i].enabled = true;

                thisBones[i].linearVelocity = Vector3.zero;
                thisBones[i].angularVelocity = Vector3.zero;
            }

            yield return waitPhysics;

            for (int i = 0; i < thisBones.Length; i++)
            {
                thisBones[i].useGravity = true;
                thisBones[i].AddForce(velocity, ForceMode.Impulse);
            }

            Deactivate();
            isRagdolled = true;
            yield return null;
        }

        public void Deactivate()
        {
            if (isDeactivated)
            {
                return;
            }

            if (deactivationCoroutine != null)
            {
                return;
            }

            deactivationCoroutine = StartCoroutine(DeactivateInternal());
        }
        private IEnumerator DeactivateInternal()
        {
            yield return waitTime;

            while (IsSleeping())
            {               
                yield return null;
            }

            for (int i = 0; i < thisBones.Length; i++)
            {
                thisBones[i].isKinematic = true;
                thisBones[i].useGravity = false;
                thisBones[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
            }

            isDeactivated = true;
        }

        public bool IsRagdolled() => isRagdolled;
        private bool IsSleeping()
        {
            for (int i = 0; i < thisBones.Length; i++)
            {
                if (!thisBones[i].IsSleeping())
                {
                    return false;
                }
            }

            return true;
        }

        public LayerMask GetCollisionMask() => collisionMask;
        public Rigidbody[] GetBones() => thisBones;
    }
}