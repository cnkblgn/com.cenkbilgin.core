using System;
using UnityEngine;
using UnityEngine.Events;
using Core;
using Core.Localization;

namespace Game
{
    using static CoreUtility;

    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Interactable : MonoBehaviour
    {
        public event Action<Interactable, GameEntity, float> OnInteractionProgress = null;
        public event Action<Interactable, GameEntity> OnInteracted = null;
        public event Action<Interactable, GameEntity> OnStayedFocus = null;
        public event Action<Interactable, GameEntity> OnEnteredFocus = null;
        public event Action<Interactable, GameEntity> OnExitedFocus = null;

        public string Description => description;
        public InteractableType Type => interactionType;
        public Collider ThisCollider => thisCollider; 
        public Transform ThisTransform => thisTransform;

        [Header("_")]
        [SerializeField] private string descriptionID = STRING_NULL;

        [Header("_")]
        [SerializeField] private InteractableType interactionType = InteractableType.DEFAULT;
        [SerializeField, Min(0)] private float interactionSpeed = 1;
        [SerializeField, ReadOnly] private float interactionProgress = 0;

        [Header("_")]
        [SerializeField] private UnityEvent onInteracted = null;

        private Transform thisTransform = null;
        private Collider thisCollider = null;
        private string description = STRING_EMPTY;
        private bool isInteractionEnabled = true;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
            thisTransform = GetComponent<Transform>();

            gameObject.SetLayer(LayerMask.NameToLayer("Interactable"));

            if (descriptionID != STRING_NULL) OverrideDescription(ManagerCoreLocalization.Instance.Get(descriptionID).ToStyle("info_default").ToYellow());
        }
#if UNITY_EDITOR
        private void Reset() => gameObject.SetLayer(LayerMask.NameToLayer("Interactable"));
#endif

        public void OnStayFocus(GameEntity entityObject)
        {
            OnStayedFocus?.Invoke(this, entityObject);
        }
        public void OnEnterFocus(GameEntity entityObject)
        {
            OnEnteredFocus?.Invoke(this, entityObject);

            if (interactionType == InteractableType.HOLD_DEFAULT)
            {
                SetInteractionProgress(0);
            }
        }
        public void OnExitFocus(GameEntity entityObject)
        {
            OnExitedFocus?.Invoke(this, entityObject);

            if (interactionType == InteractableType.HOLD_DEFAULT)
            {
                SetInteractionProgress(0);
            }
        }
        public bool OnInteract(GameEntity entityObject)
        {
            if (!isInteractionEnabled)
            {
                return false;
            }

            onInteracted?.Invoke();
            OnInteracted?.Invoke(this, entityObject);

            return true;
        }
        public float OnProgress(GameEntity entityObject)
        {
            if (!isInteractionEnabled)
            {
                return -1;
            }

            if (interactionType != InteractableType.HOLD_PERSISTENT && interactionType != InteractableType.HOLD_DEFAULT)
            {
                return -1;
            }

            interactionProgress = Mathf.Clamp01(interactionProgress + Time.deltaTime * interactionSpeed);
            OnInteractionProgress?.Invoke(this, entityObject, interactionProgress);

            return interactionProgress;
        }

        public void EnableInteraction()
        {
            isInteractionEnabled = true;
            thisCollider.enabled = isInteractionEnabled;

            SetInteractionProgress(0);
        }
        public void DisableInteraction()
        {
            isInteractionEnabled = false;
            thisCollider.enabled = isInteractionEnabled;

            SetInteractionProgress(0);
        }

        public float GetInteractionProgress() => interactionProgress;
        public void SetInteractionProgress(float value) => interactionProgress = Mathf.Clamp01(value);

        public void OverrideDescription(string value) => description = value;
    }
}