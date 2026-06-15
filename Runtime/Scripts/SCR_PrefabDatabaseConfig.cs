using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [CreateAssetMenu(fileName = "SCO_PrefabDatabase", menuName = "Resources/Prefab Database Config", order = 0)]
    public class PrefabDatabaseConfig : ScriptableObject
    {
        [Header("_")]
        [SerializeField, Required] private GameObject[] collection = null;

        public bool TryGet(PrefabID id, out GameObject prefab)
        {
            if (!PrefabDatabase.GetIsValid())
            {
                Build();
            }

            return PrefabDatabase.TryGet(id, out prefab);
        }
        public string[] GetKeys()
        {
            if (!PrefabDatabase.GetIsValid())
            {
                Build();
            }

            return PrefabDatabase.GetKeys();
        }

        [Clickable("Build")]
        public void Build() => PrefabDatabase.Build(collection);
    }
}
