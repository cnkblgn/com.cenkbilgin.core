using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    internal static class PrefabDatabase
    {
        private static Dictionary<PrefabID, GameObject> database = null;
        private static string[] keys = Array.Empty<string>();

        internal static string[] GetKeys() => keys;
        internal static bool GetIsValid() => database != null;

        internal static bool TryGet(PrefabID id, out GameObject prefab)
        {
            if (!GetIsValid())
            {
                throw new InvalidOperationException($"Database is not parsed!");
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
        internal static void Build(GameObject[] gameObjects)
        {
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