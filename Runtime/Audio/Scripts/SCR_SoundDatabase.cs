using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public static class SoundDatabase
    {
        private static readonly Dictionary<SoundID, SoundEntry> database = new();
        private static SearchCollection<string> ids = new(Array.Empty<SearchEntry<string>>());

        internal static void Build(AudioClip[] collection)
        {
            if (collection == null)
            {
                return;
            }

            database.Clear();

            ids = new SearchCollection<string>(new SearchEntry<string>[collection.Length]);

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
                ids.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"SoundDatabase build successfull!");
        }

        public static SearchCollection<string> GetIDs() => ids;
        public static SoundID GetID(int index)
        {
            if (index >= ids.Entries.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException($"sound database index out of range {nameof(index)}");
            }

            if (!database.TryGetValue(new(ids.Entries[index].Value, -1), out SoundEntry entry))
            {
                return SoundID.NONE;
            }

            return entry.ID;
        }
        public static int GetIndex(string id)
        {
            if (database.TryGetValue(new(id, -1), out SoundEntry entry))
            {
                return entry.ID.Index;
            }

            return -1;
        }
        internal static AudioClip GetClip(SoundID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{id.ToString()}] soundID is not valid!");
            }

            if (database.TryGetValue(id, out SoundEntry entry))
            {
                return entry.Clip;
            }

            Debug.LogError($"audio clip not found! [{id.Key}]");
            return null;
        }
    }
}