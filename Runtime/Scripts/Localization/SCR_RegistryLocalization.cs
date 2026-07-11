using UnityEngine;

namespace Core.Localization
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_LocalizationRegistry", menuName = "Resources/Localization Registry", order = 0)]
    internal sealed class RegistryLocalization : Registry
    {
        [Header("_")]
        [SerializeField, Required] private TextAsset file = null;

        [Clickable("Build")]
        private void Build() => LocalizationDatabase.Build(file);

        public override void OnAwake() => Build();
        public override void OnInitialize() => Build();
    }
}
