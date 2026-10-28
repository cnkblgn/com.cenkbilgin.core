using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Graphics
{
    public static class IconDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static Sprite[] sprites = Array.Empty<Sprite>();

        private static readonly Sprite defaultSprite = Sprite.Create(new(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f) );

        internal static void Build(Sprite[] _sprites)
        {
            if (_sprites == null)
            {
                return;
            }

            List<Sprite> validSprites = new(_sprites.Length);
            idLookup.Clear();

            for (int i = 0; i < _sprites.Length; i++)
            {
                Sprite sprite = _sprites[i];

                if (sprite == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Icon database sprite is invalid at index {i}!");
#endif
                    continue;
                }

                string key = sprite.name;

                idLookup[key] = validSprites.Count;
                validSprites.Add(sprite);
            }

            sprites = validSprites.ToArray();
            version++;

            Debug.Log($"Icon database build successful! Sprite count: {sprites.Length}");
        }

        public static int GetSprites() => sprites.Length;
        public static int GetIDIndex(string key) => key != null && idLookup.TryGetValue(key, out int index) ? index : -1;
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
                Debug.Log($"[{nameof(id)}] Icon id [{id}] is not valid!");
                return defaultSprite;
            }

            return GetSprite(id.Index);
        }
    }
}
