using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [SelectionBase]
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        public event Action<Collider> OnEnter = null;
        public event Action<Collider> OnExit = null;

        [Header("_")]
        [SerializeField] private LayerMask collisionMask = 0;

        [Header("_")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool triggerOnlyOnce = false;

        [Header("_")]
        [SerializeField] private UnityEvent onEnter = null;
        [SerializeField] private UnityEvent onExit = null;

        private Collider thisCollider = null;
        private bool isVisited = false;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();

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
#if UNITY_EDITOR
        private void Reset() => gameObject.SetLayer(LayerMask.NameToLayer("Trigger"));
#endif
        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnlyOnce)
            {
                if (isVisited)
                {
                    return;
                }
            }

            isVisited = true;

            onEnter?.Invoke();
            OnEnter?.Invoke(other);

            if (debugMode)
            {
                Debug.Log("TriggerZone.Enter() " + other.gameObject.name);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            onExit?.Invoke();
            OnExit?.Invoke(other);

            if (debugMode)
            {
                Debug.Log("TriggerZone.Exit() " + other.gameObject.name);
            }
        }
        private void OnDestroy()
        {
            onEnter = null;
            onExit = null;
            OnEnter = null;
            OnExit = null;
        }

        public bool GetIsVisited() => isVisited;
        public void SetIsVisited(bool value) => isVisited = value;
    }
}