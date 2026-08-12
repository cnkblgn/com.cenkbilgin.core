using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public static class SoundDatabase
    {
        private static readonly Dictionary<string, int> idLookup = new();
        private static SearchCollection<string> idSearch = new(Array.Empty<SearchEntry<string>>());
        private static AudioClip[] database = Array.Empty<AudioClip>();

        internal static void Build(AudioClip[] clipCollection)
        {
            if (clipCollection == null)
            {
                return;
            }

            database = new AudioClip[clipCollection.Length];
            idSearch = new SearchCollection<string>(new SearchEntry<string>[clipCollection.Length]);
            idLookup.Clear();

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

                idLookup[key] = i;
                database[i] = clip;
                idSearch.Entries[i] = new(key, key);
            }

            Debug.Log($"SoundDatabase build successfull!");
        }
 
        public static SearchCollection<string> GetIDs() => idSearch;
        public static SoundID GetID(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"sOUND id not found index out of range");
            }

            return new(idSearch.Entries[index].Value, index);
        }
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static AudioClip GetClip(int index)
        {
            if (index >= database.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Sound not found index out of range");
            }

            return database[index];
        }
        public static AudioClip GetClip(SoundID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException($"[{nameof(id)}] SoundID is not valid!");
            }

            return GetClip(id.Index);
        }
    }
}