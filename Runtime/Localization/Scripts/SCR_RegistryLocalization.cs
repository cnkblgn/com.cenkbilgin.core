using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_LocalizationRegistry", menuName = "Resources/Core/Localization Registry", order = 0)]
    public sealed class RegistryLocalization : Registry
    {
        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        [Header("_")]
        [SerializeReference, Reference] private LocalizationInterpolator[] interpolators;

        [Clickable("Build")]
        private void Build() => LocalizationDatabase.Build(file, interpolators);

        public override void OnBeforeSceneLoad() => Build();
        public override void OnAssemblyReload() => Build();
    }
}
