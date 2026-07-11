using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_SoundRegistry", menuName = "Resources/Sound Registry", order = 0)]
    internal sealed class RegistrySound : Registry
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip[] collection = null;

        public override void OnAwake() => SoundDatabase.Build(collection);
        public override void OnInitialize() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        internal void Generate()
        {
            SoundDatabase.Build(collection);

            GenerateScriptDatabase(this, "Core.Audio", "SoundID", SoundDatabase.GetIDs(), "SCR_GeneratedSoundID.cs", true, true);
        }
#endif
    }
}
