using UnityEngine;

namespace Core.Surface
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_SurfaceRegistry", menuName = "Resources/Core/Surface Registry", order = 10)]
    public sealed class RegistrySurface : Registry
    {
        [Header("_")]
        [SerializeField, Required] private string[] entries;

        public override void OnBeforeSceneLoad() => SurfaceDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            SurfaceDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Surface", "SurfaceTag", SurfaceDatabase.GetIDs(), "SCR_GeneratedSurfaceTag.cs", true, true, 1);
        }

        /// <summary> Overrides entries </summary>
        public void Override(string[] entries)
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
