using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<IconID, Sprite> database = new();

        internal static void Build(Sprite[] spriteCollection)
        {
            if (spriteCollection == null)
            {
                return;
            }

            database.Clear();

            idSearch = new SearchCollection<string>(new SearchEntry<string>[spriteCollection.Length]);

            for (int i = 0; i < spriteCollection.Length; i++)
            {
                Sprite clip = spriteCollection[i];
#if UNITY_EDITOR
                if (clip == null)
                {
                    Debug.LogError("Icon database sprite is null!");
                    continue;
                }
#endif
                string key = spriteCollection[i].name;
                IconID id = new(key);

                database.Add(id, spriteCollection[i]);
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }


            Debug.Log($"IconDatabase build successfull!");
        }

        public static SearchCollection<string> GetIDs() => idSearch;
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
