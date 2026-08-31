using System;
using UnityEngine;
using Core.Actors;

namespace Core.Weapon
{
    [DisallowMultipleComponent]
    public sealed class Weapon : MonoBehaviour
    {
        public WeaponID ID => id;
        public ulong Tags => tags;
        public Actor User => user;
        public WeaponSettings Settings => settings;

        [Header("_")]
        [SerializeField] private WeaponID id;

        private Actor user = null;
        private WeaponSettings settings = null;
        private IWeaponHandler handler = null;
        private ulong tags = 0;
        private bool isInitialized = false;

        public void Initialize(Actor user, WeaponSettings settings)
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
            this.handler = GetComponent<IWeaponHandler>();
            this.tags = id.GetDefinition().Tags;

            handler.HandleInitialize(this);

#if UNITY_EDITOR
            if (settings == null)
            {
                Debug.LogWarning("Weapon settings is missing! Now using default weapon settings! Ignore if its intented!");
            }
#endif
        }
    }
}