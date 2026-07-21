using System.Collections.Generic;
using UnityEngine;

namespace Core.Prefab
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_PrefabRegistry", menuName = "Resources/Core/Prefab Registry", order = 0)]
    public sealed class RegistryPrefab : Registry
    {
        [Header("_")]
        [SerializeField, Required] private List<GameObject> entries = null;

        public override void OnBeforeSceneLoad() => PrefabDatabase.Build(entries);
        public override void OnAssemblyReload() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        public void Generate()
        {
            PrefabDatabase.Build(entries);

            GenerateScriptDatabase(this, "Core.Prefab", "PrefabID", PrefabDatabase.GetIDs(), "SCR_GeneratedPrefabID.cs", true, false);
        }
        public void Register(GameObject obj) => entries.Add(obj);
        public void Unregister(GameObject obj) => entries.Remove(obj);
#endif
    }
}
