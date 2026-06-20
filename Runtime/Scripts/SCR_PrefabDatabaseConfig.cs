using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_PrefabDatabase", menuName = "Resources/Prefab Database Config", order = 0)]
    public sealed class PrefabDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private GameObject[] collection = null;

        [Clickable("Build")]
        public void Parse() => PrefabDatabase.Parse(collection);
    }
}
