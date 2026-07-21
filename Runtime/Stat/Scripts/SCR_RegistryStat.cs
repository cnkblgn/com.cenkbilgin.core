using UnityEngine;

namespace Core.Stat
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_StatRegistry", menuName = "Resources/Core/Stat Registry", order = 10)]
    public sealed class RegistryStat : Registry
    {
        [Header("_")]
        [SerializeField] private StatEntry[] entries;

        public override void OnBeforeSceneLoad() => StatDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            StatDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Stat", "StatID", StatDatabase.GetIDs(), "SCR_GeneratedStatID.cs", true, false);
        }

        /// <summary> Overrides entries </summary>
        public void Override(StatEntry[] entries)
        {
            if (entries == null)
            {
                Debug.LogError("Registry override entry cannot be null!");
                return;
            }

            this.entries = new StatEntry[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                this.entries[i] = new(entries[i]);
            }
        }
#endif
    }
}
