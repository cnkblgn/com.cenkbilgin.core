using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_IconRegistry", menuName = "Resources/Core/Icon Registry", order = 0)]
    public sealed class RegistryIcon : Registry
    {
        [Header("_")]
        [SerializeField, Required] private Sprite[] entries = null;

        public override void OnBeforeSceneLoad() => IconDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        internal void Generate()
        {
            IconDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Graphics", "IconID", IconDatabase.GetIDs(), "SCR_GeneratedIconID.cs", true, false);
        }
#endif
    }
}
