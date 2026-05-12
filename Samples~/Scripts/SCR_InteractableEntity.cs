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
    public class InteractableEntity : MonoBehaviour
    {
        public event Action<InteractableEntity, GameEntity, InteractableState> OnStateChanged = null;

        public string Description => description;
        public Collider ThisCollider => thisCollider; 
        public Transform ThisTransform => thisTransform;
        public InteractableType Type => thisHandler != null ? thisHandler.Type : InteractableType.DEFAULT;

        [Header("_")]
        [SerializeField] private LocalizedString descriptionID = default;

        [Header("_")]
        [SerializeField] private UnityEvent onInteracted = null;

        private Transform thisTransform = null;
        private Collider thisCollider = null;
        private IInteractableHandler thisHandler = null;
        private string description = STRING_EMPTY;
        private bool isInteractionEnabled = true;

        private void Awake()
        {
            thisCollider = GetComponent<Collider>();
            thisTransform = GetComponent<Transform>();
            thisHandler = GetComponent<IInteractableHandler>();

            if (descriptionID.IsValid)
            {
                OverrideDescription(descriptionID.Get().ToStyle("info_default").ToYellow());
            }

            gameObject.SetLayer(LayerMask.NameToLayer("Entity"));
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObject.SetLayer(LayerMask.NameToLayer("Entity"));
        }
#endif
        public void OnEnterFocus(GameEntity entity)
        {
            thisHandler?.HandleEnterFocus(entity);
            OnStateChanged?.Invoke(this, entity, InteractableState.ON_ENTER_FOCUS);
        }
        public void OnExitFocus(GameEntity entity)
        {
            thisHandler?.HandleExitFocus(entity);
            OnStateChanged?.Invoke(this, entity, InteractableState.ON_EXIT_FOCUS);
        }
        public bool OnInteract(GameEntity entity)
        {
            if (!isInteractionEnabled)
            {
                return false;
            }

            if (thisHandler != null && !thisHandler.HandleInteract(entity))
            {
                return false;
            }

            onInteracted?.Invoke();
            OnStateChanged?.Invoke(this, entity, InteractableState.ON_INTERACTED);
            return true;
        }

        public void EnableInteraction()
        {
            isInteractionEnabled = true;
            thisCollider.enabled = isInteractionEnabled;
        }
        public void DisableInteraction()
        {
            isInteractionEnabled = false;
            thisCollider.enabled = isInteractionEnabled;
        }

        public void OverrideDescription(string value) => description = value;
    }
}