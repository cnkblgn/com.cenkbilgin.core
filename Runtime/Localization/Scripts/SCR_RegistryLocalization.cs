using System.Collections.Generic;
using UnityEngine;

namespace Core.Localization
{
    [CreateAssetMenu(fileName = "SCO_LocalizationRegistry", menuName = "Resources/Core/Localization Registry", order = 0)]
    public sealed class RegistryLocalization : Registry
    {
        [Header("_")]
        [SerializeReference, Reference] private LocalizationInterpolator[] interpolators;

        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        private readonly List<string> extraEntries = new();

        public override void OnAfterAssembliesLoaded() => Reload();
        public override void OnAfterScriptLoad() => Reload();
        public override void Reload() => LocalizationDatabase.Build(file.text, interpolators);

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