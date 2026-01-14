using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "SCO_PrefabDatabase", menuName = "Resources/Prefab Database Config", order = 0)]
    public class PrefabDatabaseConfig : ScriptableObject
    {
        public PrefabData[] Collection => collection;

        [Header("_")]
        [SerializeField] private PrefabData[] collection = null;
    }
}
