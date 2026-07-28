using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public static class SoundDatabase
    {
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static readonly Dictionary<SoundID, SoundEntry> database = new();

        internal static void Build(AudioClip[] clipCollection)
        {
            if (clipCollection == null)
            {
                return;
            }

            database.Clear();

            idSearch = new SearchCollection<string>(new SearchEntry<string>[clipCollection.Length]);

            for (int i = 0; i < clipCollection.Length; i++)
            {
                AudioClip clip = clipCollection[i];

#if UNITY_EDITOR
                if (clip == null)
                {
                    Debug.LogError("Sound database clip is null!");
                    continue;
                }
#endif

                string key = clipCollection[i].name;
                SoundID id = new(key, i);

                database.Add(id, new(id, clipCollection[i]));
                idSearch.Entries[i] = new SearchEntry<string>(key, key);
            }

            Debug.Log($"SoundDatabase build successfull!");
        }
 
        public static SearchCollection<string> GetIDs() => idSearch;
        public static SoundID GetID(int index)
        {
            if (index >= idSearch.Entries.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException($"sound database index out of range {nameof(index)}");
            }

            if (!database.TryGetValue(new(idSearch.Entries[index].Value, -1), out SoundEntry entry))
            {
                return SoundID.NONE;
            }

            return entry.ID;
        }
        public static AudioClip GetClip(SoundID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] soundID is not valid!");
            }

            if (database.TryGetValue(id, out SoundEntry entry))
            {
                return entry.Clip;
            }

            Debug.LogError($"Audio clip not found! [{id.Key}]");
            return null;
        }
        public static int GetIndex(string key)
        {
            if (database.TryGetValue(CreateID(key), out SoundEntry entry))
            {
                return entry.ID.Index;
            }

            return -1;
        }
        internal static SoundID CreateID(string key) => new(key, -1);
    }
}