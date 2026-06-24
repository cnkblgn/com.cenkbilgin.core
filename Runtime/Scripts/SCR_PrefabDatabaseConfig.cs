using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_PrefabDatabase", menuName = "Resources/Prefab Database Config", order = 0)]
    public sealed class PrefabDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private List<GameObject> collection = null;

        [Clickable("Build")]
        public void Parse() => PrefabDatabase.Parse(collection);

#if UNITY_EDITOR
        public void Register(GameObject obj) => collection.Add(obj);
        public void Unregister(GameObject obj) => collection.Remove(obj);
#endif
    }
}
