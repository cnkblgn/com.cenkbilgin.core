using UnityEngine;

namespace Core.Damage
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_DamageRegistry", menuName = "Resources/Core/Damage Registry", order = 10)]
    public sealed class RegistryDamage : Registry
    {
        [Header("_")]
        [SerializeField, Required] private string[] entries;

        public override void OnBeforeSceneLoad() => DamageDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            DamageDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Damage", "DamageTag", DamageDatabase.GetTags(), "SCR_GeneratedDamageTag.cs", true, true, 1);
        }

        /// <summary> Overrides ids </summary>
        public void OverrideIDs(string[] entries)
        {
            if (entries == null)
            {
                Debug.LogError("Registry override entry cannot be null!");
                return;
            }

            this.entries = new string[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                this.entries[i] = entries[i];
            }
        }
#endif
    }
}
