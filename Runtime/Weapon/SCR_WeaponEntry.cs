using System;
using System.Reflection;
using UnityEngine;

namespace Core.Weapon
{
    [Serializable]
    public struct WeaponEntry
    {
#if UNITY_EDITOR
        [HideInInspector] public string Name;
#endif

        [Info("Please generate id if its not visible")]
        public WeaponID ID;
        public WeaponTag[] Tags;
        [SerializeReference, Reference] public WeaponSettings Settings;

        public WeaponEntry(WeaponID id, WeaponTag[] tags, WeaponSettings settings)
        {
            ID = id;
            Tags = tags;
            Settings = settings;

#if UNITY_EDITOR
            Name = ID.Key;
#endif
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            Name = ID.Key;
            Settings?.OnValidate();
        }
#endif
    }
}