using System;
using UnityEngine;

namespace Core
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class GameEntity : MonoBehaviour
    {
        public event Action OnActivated = null;
        public event Action OnDeactivated = null;

        public Collider ThisCollider => thisCollider;
        public bool IsPlayer => isPlayer;

        [Header("_")]
        [SerializeField] private bool isPlayer = false;

        private Collider thisCollider = null;

        protected virtual void Awake()
        {
            thisCollider = GetComponent<Collider>();

            UpdateLayer();
        }
#if UNITY_EDITOR
        private void Reset() => UpdateLayer();
#endif
        protected virtual void UpdateLayer() => gameObject.SetLayer(LayerMask.NameToLayer(isPlayer ? "Player" : "Entity"));

        public void Override(bool isPlayer)
        {
            this.isPlayer = isPlayer;
            UpdateLayer();
        }
        public void Activate()
        {
            OnActivated?.Invoke();

            thisCollider.enabled = true;           

            this.enabled = true;
        }
        public void Deactivate()
        {
            OnDeactivated?.Invoke();

            thisCollider.enabled = false;

            this.enabled = false;
        }
    }
}