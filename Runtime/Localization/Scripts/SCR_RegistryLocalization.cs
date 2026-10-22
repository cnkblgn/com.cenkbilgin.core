using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    [CreateAssetMenu(fileName = "SCO_LocalizationRegistry", menuName = "Resources/Core/Localization Registry", order = -15)]
    public sealed class RegistryLocalization : Registry
    {
        public override int Priority => -15;

        [Header("_")]
        [SerializeField, Required] private LocalizationInterpolator[] interpolators;

        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        private readonly List<string> extraEntries = new();

        public override void OnAfterAssembliesLoaded() => BuildDatabase();
        public override void OnAfterScriptLoad() => BuildDatabase();

#if UNITY_EDITOR
        public override void Reload() => BuildDatabase();
#endif

        public void BuildDatabase()
        {
            LocalizationDatabase.Build(file.text, interpolators);

            foreach (string csvContent in extraEntries)
            {
                LocalizationDatabase.Merge(csvContent);
            }
        }
        public void OverrideEntries(string csvContent)
        {
            if (string.IsNullOrEmpty(csvContent))
            {
                Debug.LogError("Localization override failed! content missing!?");
                return;
            }

            LocalizationDatabase.Build(csvContent, interpolators);
        }
        /// <summary> Does not support new added languages! only existing ones </summary>
        public void AppendEntries(string csvContent)
        {
            if (string.IsNullOrEmpty(csvContent))
            {
                Debug.LogError("Failed to append localization entries. content is empty.");
                return;
            }

            extraEntries.Add(csvContent);
        }
    }
}