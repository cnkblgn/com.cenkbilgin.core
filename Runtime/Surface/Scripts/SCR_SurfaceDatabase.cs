using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Surface
{
    public static class SurfaceDatabase
    {
        private static readonly Dictionary<string, int> tagLookup = new();
        private static SurfaceTag[] database = Array.Empty<SurfaceTag>();

        internal static void Build(string[] tagCollection)
        {
            if (tagCollection == null)
            {
                return;
            }

            tagLookup.Clear();
            database = new SurfaceTag[tagCollection.Length + 1];
            database[0] = new("GENERIC", 0);

            for (int i = 0; i < tagCollection.Length; i++)
            {
                string key = tagCollection[i];
                int index = i + 1;

                if (string.IsNullOrEmpty(key))
                {
                    Debug.LogError("Surface database tag key is invalid!?");
                    continue;
                }

                tagLookup[key] = index;
                database[index] = new(key, index);
            }

            Debug.Log($"Surface database build successfull!");
        }

        public static IReadOnlyList<SurfaceTag> GetTags() => database;
        public static SurfaceTag GetTag(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Surface  tag not found index out of range");
            }

            return database[index];
        }
        public static int GetTagIndex(string key) => tagLookup.TryGetValue(key, out int index) ? index : -1;
    }
}
