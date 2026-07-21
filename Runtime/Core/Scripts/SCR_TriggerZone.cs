using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [SelectionBase]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TriggerZone : MonoBehaviour
    {
        public event Action<TriggerContext> OnStateChanged = null;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = 0;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool onlyOnce = false;

        [Header("_")]
        [SerializeField] private UnityEvent onEnter = null;
        [SerializeField] private UnityEvent onExit = null;

        private BoxCollider thisCollider = null;
        private bool isVisited = false;

        private void Awake()
        {
            thisCollider = GetComponent<BoxCollider>();
            thisCollider.isTrigger = true;
            thisCollider.includeLayers = collisionMask;
            thisCollider.excludeLayers = ~collisionMask;

            gameObject.SetLayer(LayerMask.NameToLayer("Trigger"));
        }
        private IEnumerator Start()
        {
            thisCollider.enabled = false;

            yield return null;
            yield return null;

            thisCollider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (onlyOnce)
            {
                if (isVisited)
                {
                    return;
                }
            }

            isVisited = true;

            onEnter?.Invoke();
            OnStateChanged?.Invoke(new(TriggerState.ENTERED, other));

            if (debugMode)
            {
                Debug.Log("TriggerZoneEnter: " + other.gameObject.name);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            onExit?.Invoke();
            OnStateChanged?.Invoke(new(TriggerState.EXITED, other));

            if (debugMode)
            {
                Debug.Log("TriggerZoneExit: " + other.gameObject.name);
            }
        }

        public bool GetIsVisited() => isVisited;
        public void SetIsVisited(bool value) => isVisited = value;

#if UNITY_EDITOR
        private void EnsureCollider()
        {
            if (thisCollider == null)
            {
                thisCollider = GetComponent<BoxCollider>();
            }
        }
        public Bounds GetBounds()
        {
            EnsureCollider();

            return thisCollider.bounds;
        }
        public Vector3 GetSize()
        {
            EnsureCollider();

            return thisCollider.size;
        }
        public Vector3 GetCenter()
        {
            EnsureCollider();

            return thisCollider.center;
        }
#endif
    }
}