using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class ManagerCoreLocalization : Manager<ManagerCoreLocalization>
    {
        public static event Action<int> OnLocalizationChanged = null;

        [Header("_")]
        [SerializeField, Required] private LocalizationDatabaseConfig database = null;

        private Dictionary<string, string> language = null;
        private int languageIndex = -1;
        private bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RESET() => OnLocalizationChanged = null;

        private void Validate()
        {
            if (isInitialized)
            {
                return;
            }

            Initialize();
            isInitialized = true;
        }
        private void Initialize()
        {
            if (database == null)
            {
                throw new NullReferenceException($"ManagerCoreLocalization.SetLanguage() [{nameof(database)}]");
            }

            languageIndex = languageIndex == -1 ? 0 : languageIndex;
            language = database.GetLanguage(languageIndex);

            if (language == null)
            { 
                throw new InvalidOperationException($"ManagerCoreLocalization.SetLanguage() language missing for index {languageIndex}.");
            }
        }
         
        public IReadOnlyCollection<string> GetLanguages() => database.Languages;
        public string GetLanguage() => database.Languages[languageIndex];
        public void SetLanguage(int index)
        {
            Validate();

            if (this.languageIndex == index || index < 0 || index >= database.Languages.Length)
            {
                return;
            }

            this.languageIndex = index;
            language = database.GetLanguage(index);
            OnLocalizationChanged?.Invoke(this.languageIndex);
        }

        public string Get(string key)
        {
            Validate();

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
            Validate();

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
            Validate();

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