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

        public override void OnBeforeSceneLoad() => Reload();
        public override void OnAfterScriptLoad() => Reload();

        public override void Reload() => LocalizationDatabase.Build(file, interpolators);

    }
}
