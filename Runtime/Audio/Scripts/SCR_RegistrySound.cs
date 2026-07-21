using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_SoundRegistry", menuName = "Resources/Core/Sound Registry", order = 0)]
    public sealed class RegistrySound : Registry
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip[] entries = null;

        public override void OnBeforeSceneLoad() => SoundDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        internal void Generate()
        {
            SoundDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Audio", "SoundID", SoundDatabase.GetIDs(), "SCR_GeneratedSoundID.cs", true, true);
        }
#endif
    }
}
