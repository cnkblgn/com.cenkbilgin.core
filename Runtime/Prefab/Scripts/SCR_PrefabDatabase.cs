using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Prefab
{
    public static class PrefabDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static GameObject[] prefabs = Array.Empty<GameObject>();

        internal static void Build(GameObject[] _prefabs)
        {
            if (_prefabs == null)
            {
                return;
            }

            List<GameObject> validPrefabs = new(_prefabs.Length);
            idLookup.Clear();

            for (int i = 0; i < _prefabs.Length; i++)
            {
                GameObject prefab = _prefabs[i];

                if (prefab == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Prefab database prefab is invalid at index {i}!");
#endif
                    continue;
                }

                string key = prefab.name;

                idLookup[key] = validPrefabs.Count;
                validPrefabs.Add(prefab);
            }

            prefabs = validPrefabs.ToArray();
            version++;

            Debug.Log($"Prefab database build successful! Prefab count: {prefabs.Length}");
        }
        internal static void Build(List<GameObject> gameObjectCollection) => Build(gameObjectCollection.ToArray());

        public static int GetPrefabs() => prefabs.Length;
        public static int GetIDIndex(string key) => key != null && idLookup.TryGetValue(key, out int index) ? index : -1;
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
                throw new ArgumentNullException(nameof(id), $"Prefab id [{id}] is not valid!");
            }

            return GetPrefab(id.Index);
        }
        public static GameObject SpawnPrefab(PrefabID id, Vector3 position, Quaternion rotation, Transform parent) => GameObject.Instantiate(GetPrefab(id), position, rotation, parent);
    }
}