using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class PrefabDatabase
    {
        internal static bool IsParsed => database != null;

        private static Dictionary<PrefabID, GameObject> database = null;
        private static string[] keys = Array.Empty<string>();

        public static string[] GetKeys() => keys;

        internal static bool TryGet(PrefabID id, out GameObject prefab)
        {
            if (!IsParsed)
            {
                throw new InvalidOperationException($"Prefab database is not parsed! Please build via PrefabDatabaseConfig");
            }

            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] prefabID is not valid!");
            }

            if (database.TryGetValue(id, out prefab))
            {
                return true;
            }

            Debug.LogError($"prefab not found! [{id.Key}]");
            return false;
        }
        internal static bool TrySpawn(PrefabID id, Vector3 position, Quaternion rotation, Transform parent, out GameObject gameObject)
        {
            gameObject = null;

            if (TryGet(id, out GameObject prefab))
            {
                gameObject = GameObject.Instantiate(prefab, position, rotation, parent);
                return true;
            }

            return false;
        }
        internal static void Parse(GameObject[] gameObjects)
        {
            if (gameObjects == null)
            {
                return;
            }

            database = new(gameObjects.Length);

            keys = new string[gameObjects.Length];

            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject obj = gameObjects[i];

#if UNITY_EDITOR
                if (obj == null)
                {
                    Debug.LogError("Prefab database object is null!");
                    continue;
                }
#endif

                string key = gameObjects[i].name;

                database.Add(new(key), gameObjects[i]);

                keys[i] = key;
            }

            Debug.Log($"Prefab build successfull!");
        }
    }
}