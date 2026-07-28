using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Prefab
{
    public static class PrefabDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<PrefabID, GameObject> database = new();

        internal static void Build(GameObject[] gameObjectCollection)
        {
            if (gameObjectCollection == null)
            {
                return;
            }

            database.Clear();

            idSearch = new SearchCollection<string>(new SearchEntry<string>[gameObjectCollection.Length]);

            for (int i = 0; i < gameObjectCollection.Length; i++)
            {
                GameObject obj = gameObjectCollection[i];

#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug.LogError("Prefab database object is null!");
                    continue;
                }
#endif

                string key = gameObjectCollection[i].name;

                database.Add(new(key), gameObjectCollection[i]);

                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"Prefab build successfull!");
        }
        internal static void Build(List<GameObject> gameObjectCollection) => Build(gameObjectCollection.ToArray());

        public static SearchCollection<string> GetIDs() => idSearch;
        internal static GameObject GetPrefab(PrefabID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] prefabID is not valid!");
            }

            if (database.TryGetValue(id, out GameObject prefab))
            {
                return prefab;
            }

            Debug.LogError($"Prefab not found in database! [{id.Key}]");
            return null;
        }
        internal static GameObject SpawnPrefab(PrefabID id, Vector3 position, Quaternion rotation, Transform parent) => GameObject.Instantiate(GetPrefab(id), position, rotation, parent);
    }
}