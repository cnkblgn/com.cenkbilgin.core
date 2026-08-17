using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        private static readonly Dictionary<string, int> tagLookup = new();
        private static DamageTag[] tags = Array.Empty<DamageTag>();

        internal static void Build(string[] _tags)
        {
            if (_tags == null)
            {
                return;
            }

            tagLookup.Clear();
            tags = new DamageTag[_tags.Length + 1];
            tags[0] = DamageTag.GENERIC;
            tagLookup[DamageTag.GENERIC.Key] = 0;

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i + 1;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Damage database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            Debug.Log($"Damage database build successfull!");
        }

        public static IReadOnlyList<DamageTag> GetTags() => tags;
        public static DamageTag GetTag(int index)
        {
            if (index >= tags.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Damage tag not found index out of range");
            }

            return tags[index];
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}