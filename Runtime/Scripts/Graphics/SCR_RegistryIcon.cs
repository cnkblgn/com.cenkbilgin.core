using UnityEngine;

namespace Core.Graphics
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_IconRegistry", menuName = "Resources/Icon Registry", order = 0)]
    internal sealed class RegistryIcon : Registry
    {
        [Header("_")]
        [SerializeField, Required] private Sprite[] collection = null;

        public override void OnAwake() => IconDatabase.Build(collection);
        public override void OnInitialize() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        internal void Generate()
        {
            IconDatabase.Build(collection);

            GenerateScriptDatabase(this, "Core.Graphics", "IconID", IconDatabase.GetIDs(), "SCR_GeneratedIconID.cs", true, false);
        }
#endif
    }
}
