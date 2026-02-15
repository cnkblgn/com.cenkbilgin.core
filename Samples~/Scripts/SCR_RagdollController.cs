using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Game
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class RagdollController : MonoBehaviour
    {
        public event Action OnActivated = null;
        public event Action OnCleared = null;

        [Header("_")]
        [SerializeField] private bool disableCollidersOnAwake = false;
        [SerializeField] private bool enableCollidersOnClear = true;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = -1;

        private PhysicsBone[] thisBones = null;
        private Coroutine activationCoroutine = null;
        private Coroutine deactivationCoroutine = null;
        private readonly Dictionary<Collider, Rigidbody> mappedBones = new(2);
        private readonly WaitForSeconds waitTime = new(10);
        private readonly WaitForFixedUpdate waitPhysics = new();
        private bool isDeactivated = false;
        private bool isRagdolled = false;

        private void Awake()
        {
            Rigidbody[] temp = GetComponentsInChildren<Rigidbody>();

            thisBones = new PhysicsBone[temp.Length];

            for (int i = 0; i < thisBones.Length; i++)
            {
                PhysicsBone bone = new(temp[i]);

                bone.Collider.includeLayers = collisionMask;
                bone.Collider.excludeLayers = ~collisionMask;
                bone.Collider.enabled = !disableCollidersOnAwake;

                bone.Rigidbody.isKinematic = true;
                bone.Rigidbody.useGravity = false;
                bone.Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                
                mappedBones.Add(bone.Collider, bone.Rigidbody);
                thisBones[i] = bone;
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
                thisBones[0].Rigidbody.AddExplosionForce(force, position, radius, force, ForceMode.Impulse);
                return;
            }

            if (collider == null)
            {
                collider = thisBones[0].Collider;
            }

            if (!mappedBones.TryGetValue(collider, out var rigidbody))
            {
                Debug.LogError("RagdollController.Hit() rigidbody == null!", collider);
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

            for (int i = 0; i < thisBones.Length; i++)
            {
                PhysicsBone bone = thisBones[i];

                bone.Rigidbody.angularVelocity = Vector3.zero;
                bone.Rigidbody.linearVelocity = Vector3.zero;
                bone.Rigidbody.isKinematic = true;
                bone.Rigidbody.useGravity = false;

                bone.Collider.enabled = enableCollidersOnClear;
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

            for (int i = 0; i < thisBones.Length; i++)
            {
                PhysicsBone bone = thisBones[i];

                bone.Rigidbody.isKinematic = false;
                bone.Rigidbody.linearVelocity = Vector3.zero;
                bone.Rigidbody.angularVelocity = Vector3.zero;

                bone.Collider.enabled = true;
            }

            yield return waitPhysics;

            for (int i = 0; i < thisBones.Length; i++)
            {
                PhysicsBone bone = thisBones[i];

                bone.Rigidbody.useGravity = true;
                bone.Rigidbody.AddForce(velocity, ForceMode.VelocityChange);
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
                PhysicsBone bone = thisBones[i];

                bone.Rigidbody.isKinematic = true;
                bone.Rigidbody.useGravity = false;
                bone.Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }

            isDeactivated = true;
        }

        public bool IsRagdolled() => isRagdolled;
        private bool IsSleeping()
        {
            for (int i = 0; i < thisBones.Length; i++)
            {
                if (!thisBones[i].Rigidbody.IsSleeping())
                {
                    return false;
                }
            }

            return true;
        }

        public LayerMask GetCollisionMask() => collisionMask;
        public PhysicsBone[] GetBones() => thisBones;
    }
}