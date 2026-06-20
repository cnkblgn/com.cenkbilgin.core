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

        public readonly string Get() => LocalizationDatabase.GetString(key);
        public readonly string Get(string arg0) => LocalizationDatabase.GetString(key, arg0);
        public readonly string Get(params object[] args) => LocalizationDatabase.GetString(key, args);

        public readonly override string ToString() => Get();
    }
}