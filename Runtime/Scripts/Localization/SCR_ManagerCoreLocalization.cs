using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public sealed class ManagerCoreLocalization : Manager<ManagerCoreLocalization>
    {
        public static event Action<int> OnLocalizationChanged = null;

        [Header("_")]
        [SerializeField, Required] private LocalizationDatabaseConfig database = null;

        private Dictionary<string, string> language = null;
        private int languageIndex = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnRuntimeInitialize() => OnLocalizationChanged = null;

        protected override void Awake()
        {
            base.Awake();

            if (database == null)
            {
                throw new NullReferenceException($"[{nameof(database)}]");
            }

            database.TryParse();

            languageIndex = languageIndex == -1 ? 0 : languageIndex;
            language = database.GetLanguage(languageIndex);

            if (language == null)
            {
                throw new InvalidOperationException($"Language missing for index {languageIndex}.");
            }
        }

        public IReadOnlyCollection<string> GetLanguages() => database.GetLanguages();
        public string GetLanguage() => database.GetLanguages()[languageIndex];
        public void SetLanguage(int index)
        {
            if (this.languageIndex == index || index < 0 || index >= database.GetLanguages().Length)
            {
                return;
            }

            this.languageIndex = index;
            language = database.GetLanguage(index);
            OnLocalizationChanged?.Invoke(this.languageIndex);
        }

        public string Get(string key)
        {
            if (language.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return format;
                }
            }

            Debug.LogWarning($"Missing localization for '{key}' in '{GetLanguage()}'");
            return $"[{key}]";
        }
        public string Get(string key, string arg0)
        {
            if (language.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return string.Format(format, arg0);
                }
            }

            Debug.LogWarning($"Missing localization for '{key}' in '{GetLanguage()}'");
            return $"[{key}]";
        }
        public string Get(string key, params object[] args)
        {
            if (language.TryGetValue(key, out var format))
            {
                if (!string.IsNullOrEmpty(format))
                {
                    return args.Length > 0 ? string.Format(format, args) : format;
                }
            }

            Debug.LogWarning($"Missing localization for '{key}' in '{GetLanguage()}'");
            return $"[{key}]";
        }
    }
}