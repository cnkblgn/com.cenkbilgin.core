using System;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [Serializable]
    public struct LocalizedID : IEquatable<LocalizedID>
    {
        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public LocalizedID(string key) => this.key = key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(LocalizedID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is LocalizedID other && Equals(other);
        public static bool operator ==(LocalizedID left, LocalizedID right) => left.Equals(right);
        public static bool operator !=(LocalizedID left, LocalizedID right) => !left.Equals(right);
        public readonly override string ToString() => Get();

        public readonly string Get() => LocalizationDatabase.GetString(key);
        public readonly string Get(string arg0) => LocalizationDatabase.GetString(key, arg0);
        public readonly string Get(string arg0, string arg1) => LocalizationDatabase.GetString(key, arg0, arg1);
        public readonly string Get(params object[] args) => LocalizationDatabase.GetString(key, args);

        public readonly string GetInterpolated() => LocalizationDatabase.GetStringInterpolated(key);
        public readonly string GetInterpolated(string arg0) => LocalizationDatabase.GetStringInterpolated(key, arg0);
        public readonly string GetInterpolated(string arg0, string arg1) => LocalizationDatabase.GetStringInterpolated(key, arg0, arg1);
        public readonly string GetInterpolated(params object[] args) => LocalizationDatabase.GetStringInterpolated(key, args);

    }
}