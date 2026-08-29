using System;
using UnityEngine;
using Core.Actors;

namespace Core.Weapon
{
    [DisallowMultipleComponent]
    public sealed class WeaponEntity : MonoBehaviour
    {
        public WeaponID ID => id;
        public Actor User => user;
        public WeaponSettings Settings => settings;

        [Header("_")]
        [SerializeField] private WeaponID id;

        private Actor user = null;
        private WeaponSettings settings = null;
        private IWeaponHandler[] handlers = null;
        private bool isInitialized = false;

        public void Initialize(Actor user, WeaponSettings settings = null)
        {
            if (isInitialized)
            {
                return;
            }

            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(user), "Weapon initialize failed! weapon id is not valid!?");
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "Weapon initialize failed! user is null!?");
            }

            isInitialized = true;

            this.user = user;
            this.settings = settings ?? id.GetDefinition().ExportSettings();
            this.handlers = GetComponents<IWeaponHandler>();

            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i].Initialize(this);
            }
        }
        public bool TryGetHandler<T>(out T handler) where T : IWeaponHandler
        {        
            if (!isInitialized)
            {
                handler = default;
                Debug.LogError("You are trying to get handler with uninitialized weapon! please initialize first!");
                return false;
            }

            for (int i = 0; i < handlers.Length; i++)
            {
                if (handlers[i] is T typedHandler)
                {
                    handler = typedHandler;
                    return true;
                }
            }

            handler = default;
            return false;
        }
    }
}
