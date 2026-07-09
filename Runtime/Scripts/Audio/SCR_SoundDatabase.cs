using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public static class SoundDatabase
    {
        internal static bool IsParsed => database != null;

        private static Dictionary<SoundID, SoundEntry> database = null;
        private static string[] ids = Array.Empty<string>();

        public static IReadOnlyList<string> GetIDs() => ids;
        public static SoundID GetID(int index)
        {
            if (index >= ids.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException($"sound database index out of range {nameof(index)}");
            }

            if (!database.TryGetValue(new(ids[index], -1), out SoundEntry entry))
            {
                return SoundID.Empty;
            }

            return entry.ID;
        }
        public static int GetIndex(SoundID id)
        {
            if (!IsParsed)
            {
                Debug.LogError($"Sound database is not parsed! Please build via SoundDatabaseConfig");
                return -1;
            }

            if (database.TryGetValue(id, out SoundEntry entry))
            {
                return entry.ID.Index;
            }

            return -1;
        }

        internal static bool TryGet(SoundID id, out AudioClip clip)
        {
            clip = null;

            if (!IsParsed)
            {
                throw new InvalidOperationException($"Sound database is not parsed! Please build via SoundDatabaseConfig");
            }

            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] soundID is not valid!");
            }

            if (database.TryGetValue(id, out SoundEntry entry))
            {
                clip = entry.Clip;
                return true;
            }

            Debug.LogError($"audio clip not found! [{id.Key}]");
            return false;
        }
        internal static void Build(AudioClip[] collection)
        {
            if (collection == null)
            {
                return;
            }

            database = new(collection.Length);
            ids = new string[collection.Length];

            for (int i = 0; i < collection.Length; i++)
            {
                AudioClip clip = collection[i];

#if UNITY_EDITOR
                if (clip == null)
                {
                    Debug.LogError("Sound database clip is null!");
                    continue;
                }
#endif

                string key = collection[i].name;
                SoundID id = new(key, i);

                database.Add(id, new(id, collection[i]));
                ids[i] = key;
            }

            Debug.Log($"SoundDatabase build successfull!");
        }
    }
}