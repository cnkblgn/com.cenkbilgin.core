using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static Sprite[] sprites = Array.Empty<Sprite>();

        internal static void Build(Sprite[] _sprites)
        {
            if (_sprites == null)
            {
                return;
            }

            sprites = new Sprite[_sprites.Length];
            idLookup.Clear();

            for (int i = 0; i < _sprites.Length; i++)
            {
                Sprite clip = _sprites[i];
#if UNITY_EDITOR
                if (clip == null)
                {
                    Debug.LogError("Icon database sprite is invalid!");
                    continue;
                }
#endif
                string key = _sprites[i].name;
                IconID id = new(key, i);

                idLookup[key] = i;
                sprites[i] = _sprites[i];
            }

            Debug.Log($"Icon database build successfull!");
        }

        public static int GetSprites() => sprites.Length;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static IconID GetID(int index)
        {
            if (index >= sprites.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Icon id not found index out of range");
            }

            return new(sprites[index].name, index);
        }
        public static Sprite GetSprite(int index)
        {
            if (index >= sprites.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Icon not found index out of range");
            }

            return sprites[index];
        }
        public static Sprite GetSprite(IconID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] iconID is not valid!");
            }

            return GetSprite(id.Index);
        }
    }
}
