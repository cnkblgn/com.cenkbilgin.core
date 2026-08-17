using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static readonly Dictionary<string, int> tagLookup = new();
        private static SurfaceTag[] tags = Array.Empty<SurfaceTag>();

        internal static void Build(string[] _tags)
        {
            if (_tags == null)
            {
                return;
            }

            tagLookup.Clear();
            tags = new SurfaceTag[_tags.Length + 1];
            tags[0] = SurfaceTag.GENERIC;
            tagLookup[SurfaceTag.GENERIC.Key] = 0;

            for (int i = 0; i < _tags.Length; i++)
            {
                string key = _tags[i];
                int index = i + 1;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Surface database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                tags[index] = new(key, index);
            }

            Debug.Log($"Surface database build successfull!");
        }

        public static IReadOnlyList<SurfaceTag> GetTags() => tags;
        public static SurfaceTag GetTag(int index)
        {
            if (index >= tags.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Surface  tag not found index out of range");
            }

            return tags[index];
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}
