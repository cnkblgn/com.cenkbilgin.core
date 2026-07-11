using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_PrefabRegistry", menuName = "Resources/Prefab Registry", order = 0)]
    public sealed class RegistryPrefab : Registry
    {
        [Header("_")]
        [SerializeField, Required] private List<GameObject> collection = null;

        public override void OnAwake() => PrefabDatabase.Build(collection);
        public override void OnInitialize() => Generate();

#if UNITY_EDITOR
        [Clickable("Build")]
        internal void Generate()
        {
            PrefabDatabase.Build(collection);

            GenerateScriptDatabase(this, "Core", "PrefabID", PrefabDatabase.GetIDs(), "SCR_GeneratedPrefabID.cs", true, false);
        }
        public void Register(GameObject obj) => collection.Add(obj);
        public void Unregister(GameObject obj) => collection.Remove(obj);
#endif
    }
}
