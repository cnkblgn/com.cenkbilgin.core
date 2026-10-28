using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Damage
{
    public static class DamageDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> tagLookup = new();
        private static DamageTag[] tags = Array.Empty<DamageTag>();

        internal static void Build(string[] _tags)
        {
            if (_tags == null)
            {
                return;
            }

            if (_tags.Length > 63)
            {
                throw new InvalidOperationException("Damage database supports a maximum of 63 non-generic tags.");
            }

            tagLookup.Clear();
            tags = new DamageTag[_tags.Length];

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Damage database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            version++;
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
        public static int GetTagIndex(string key) => key != null && tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}