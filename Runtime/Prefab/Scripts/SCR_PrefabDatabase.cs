using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Prefab
{
    public static class PrefabDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static GameObject[] database = Array.Empty<GameObject>();

        internal static void Build(GameObject[] gameObjectCollection)
        {
            if (gameObjectCollection == null)
            {
                return;
            }

            idLookup.Clear();
            idSearch = new SearchCollection<string>(new SearchEntry<string>[gameObjectCollection.Length]);
            database = new GameObject[gameObjectCollection.Length];

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

                string key = obj.name;

                idLookup[key] = i;
                idSearch.Entries[i] = new(key, key);
                database[i] = obj;
            }

            Debug.Log($"Prefab build successfull!");
        }
        internal static void Build(List<GameObject> gameObjectCollection) => Build(gameObjectCollection.ToArray());

        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIndex(string id)
        {
            if (idLookup.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        internal static GameObject GetPrefab(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Prefab not found index out of range");
            }

            return database[index];
        }
        internal static GameObject GetPrefab(PrefabID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] prefabID is not valid!");
            }

            return GetPrefab(id.Index);
        }
        internal static GameObject SpawnPrefab(PrefabID id, Vector3 position, Quaternion rotation, Transform parent) => GameObject.Instantiate(GetPrefab(id), position, rotation, parent);
    }
}