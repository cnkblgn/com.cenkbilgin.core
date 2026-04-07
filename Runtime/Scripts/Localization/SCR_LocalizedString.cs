using System;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [Serializable]
    public struct LocalizedString
    {
        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public LocalizedString(string key) => this.key = key;

        public readonly string Get()
        {
            return ManagerCoreLocalization.HasInstance ? ManagerCoreLocalization.Instance.Get(key) : STRING_NULL;
        }
        public readonly string Get(string arg0)
        {
            return ManagerCoreLocalization.HasInstance ? ManagerCoreLocalization.Instance.Get(key, arg0) : STRING_NULL;
        }
        public readonly string Get(params object[] args)
        {
            return ManagerCoreLocalization.HasInstance ? ManagerCoreLocalization.Instance.Get(key, args) : STRING_NULL;
        }

        public readonly override string ToString() => Get();
    }
}