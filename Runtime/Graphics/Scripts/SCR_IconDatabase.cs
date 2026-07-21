using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        private static readonly Dictionary<IconID, Sprite> database = new();
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());

        internal static void Build(Sprite[] collection)
        {
            if (collection == null)
            {
                return;
            }

            database.Clear();

            ids = new SearchCollection<string>(new SearchEntry<string>[collection.Length]);

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
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }


            Debug.Log($"IconDatabase build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static Sprite GetSprite(IconID id)
        {
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
    }
}
