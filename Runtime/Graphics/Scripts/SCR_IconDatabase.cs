using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<string, int> idLookup = new();
        private static Sprite[] database = Array.Empty<Sprite>();

        internal static void Build(Sprite[] spriteCollection)
        {
            if (spriteCollection == null)
            {
                return;
            }

            database = new Sprite[spriteCollection.Length];
            idSearch = new SearchCollection<string>(new SearchEntry<string>[spriteCollection.Length]);
            idLookup.Clear();

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
                IconID id = new(key, i);

                idLookup[key] = i;
                database[i] = spriteCollection[i];
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }


            Debug.Log($"IconDatabase build successfull!");
        }

        public static SearchCollection<string> GetIDs() => idSearch;
        public static int GetIndex(string id)
        {
            if (idLookup.TryGetValue(id, out int a))
            {
                return a;
            }

            return -1;
        }
        public static IconID GetID(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Icon id not found index out of range");
            }

            string key = idSearch.Entries[index].Value;

            return new(key, GetIndex(key));
        }

        public static Sprite GetSprite(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Icon not found index out of range");
            }

            return database[index];
        }
        public static Sprite GetSprite(string id) => GetSprite(GetIndex(id));
        public static Sprite GetSprite(IconID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] iconID is not valid!");
            }

            return GetSprite(id);
        }
    }
}
