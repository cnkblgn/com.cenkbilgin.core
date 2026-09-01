using System;
using System.Collections.Generic;
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

        [Header("_")]
        [SerializeField] private WeaponID id;

        private Actor user;
        private WeaponSettings settings;
        private IWeaponModule[] modules;
        private ulong tags;
        private bool isInitialized;

        private void Awake()
        {
            settings = id.GetDefinition().ExportSettings();
            modules = GetComponents<IWeaponModule>();

            for (int i = 0; i < modules.Length; i++)
            {
                modules[i].Initialize(this);
            }
        }
        private void Update()
        {
            float deltaTime = Time.deltaTime;

            for (int i = 0; i < modules.Length; i++)
            {
                modules[i].Tick(deltaTime);
            }
        }

        public void Initialize(Actor user)
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
            this.tags = id.GetDefinition().Tags;
        }

        public WeaponSettings GetSettings() => settings;
        public void SetSettings(WeaponSettings settings)
        {
            this.settings = settings.Clone() ?? this.settings;

            for (int i = 0; i < modules.Length; i++)
            {
                modules[i].Initialize(this);
            }

#if UNITY_EDITOR
            if (settings == null)
            {
                Debug.LogWarning("Weapon settings is missing! Now using default weapon settings! Ignore if its intented!");
            }
#endif
        }

        public IReadOnlyList<IWeaponModule> GetModules() => modules;
        public bool TryGetModule<T>(out T module) where T : IWeaponModule
        {
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] is T result)
                {
                    module = result;
                    return true;
                }
            }

            module = default;
            return false;
        }
    }
}