using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        internal static bool IsParsed => database != null;

        private static Dictionary<IconID, Sprite> database = null;
        private static string[] ids = Array.Empty<string>();

        public static IReadOnlyList<string> GetIDs() => ids;
        public static Sprite Get(IconID id)
        {
            if (!IsParsed)
            {
                throw new InvalidOperationException($"Icon database is not parsed! Please build via IconDatabaseConfig");
            }

            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] iconID is not valid!");
            }

            if (database.TryGetValue(id, out Sprite sprite))
            {
                return sprite;
            }

            Debug.LogError($"icon not found! [{id.Key}]");
            return null;
        }

        internal static void Build(Sprite[] collection)
        {
            if (collection == null)
            {
                return;
            }

            database = new(collection.Length);
            ids = new string[collection.Length];

            for (int i = 0; i < collection.Length; i++)
            {
                Sprite clip = collection[i];

#if UNITY_EDITOR
                if (clip == null)
                {
                    Debug.LogError("Icon database sprite is null!");
                    continue;
                }
#endif

                string key = collection[i].name;
                IconID id = new(key);

                database.Add(id, collection[i]);
                ids[i] = key;
            }

            Debug.Log($"IconDatabase build successfull!");
        }
    }
}
