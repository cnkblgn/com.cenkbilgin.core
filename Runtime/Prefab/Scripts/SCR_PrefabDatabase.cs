using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Prefab
{
    public static class PrefabDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static GameObject[] prefabs = Array.Empty<GameObject>();

        internal static void Build(GameObject[] _prefabs)
        {
            if (_prefabs == null)
            {
                return;
            }

            idLookup.Clear();
            prefabs = new GameObject[_prefabs.Length];

            for (int i = 0; i < _prefabs.Length; i++)
            {
                GameObject gameObject = _prefabs[i];

#if UNITY_EDITOR
                if (gameObject == null)
                {
                    Debug.LogError("Prefab database gameObject is invalid!");
                    continue;
                }
#endif

                string key = gameObject.name;

                idLookup[key] = i;
                prefabs[i] = gameObject;
            }

            Debug.Log($"Prefab build successfull!");
        }
        internal static void Build(List<GameObject> gameObjectCollection) => Build(gameObjectCollection.ToArray());

        public static int GetPrefabs() => prefabs.Length;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static PrefabID GetID(int index)
        {
            if (index >= prefabs.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Prefab id not found index out of range");
            }

            return new(prefabs[index].name, index);
        }
        public static GameObject GetPrefab(int index)
        {
            if (index >= prefabs.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Prefab not found index out of range");
            }

            return prefabs[index];
        }
        public static GameObject GetPrefab(PrefabID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] prefabID is not valid!");
            }

            return GetPrefab(id.Index);
        }
        public static GameObject SpawnPrefab(PrefabID id, Vector3 position, Quaternion rotation, Transform parent) => GameObject.Instantiate(GetPrefab(id), position, rotation, parent);
    }
}