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
        public GameUser ThisUser => thisUser;

        [Header("_")]
        [SerializeField] private GameUser thisUser = GameUser.NONE;

        private Collider thisCollider = null;

        protected virtual void Awake()
        {
            thisCollider = GetComponent<Collider>();

            UpdateLayer();
        }
#if UNITY_EDITOR
        private void Reset() => UpdateLayer();
#endif
        protected virtual void UpdateLayer()
        {
            switch (thisUser)
            {
                case GameUser.NONE:
                    break;
                case GameUser.PLAYER:
                    gameObject.SetLayer(LayerMask.NameToLayer("Player"));
                    break;
                case GameUser.ENTITY:
                    gameObject.SetLayer(LayerMask.NameToLayer("Entity"));
                    break;
            }
        }
        public void Override(GameUser value)
        {
            thisUser = value;

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