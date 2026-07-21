using UnityEngine;

namespace Core.Effect
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_EffectRegistry", menuName = "Resources/Core/Effect Registry", order = 10)]
    public sealed class RegistryEffect : Registry
    {
        [Header("_")]
        [SerializeField] private EffectEntry[] entries;

        public override void OnBeforeSceneLoad() => EffectDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            EffectDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Effect", "EffectID", EffectDatabase.GetIDs(), "SCR_GeneratedEffectID.cs", true, true);
        }

        /// <summary> Overrides entries </summary>
        public void Override(EffectEntry[] entries)
        {
            if (entries == null)
            {
                Debug.LogError("Registry override entry cannot be null!");
                return;
            }

            this.entries = new EffectEntry[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                this.entries[i] = new(entries[i]);
            }
        }
#endif
    }
}
