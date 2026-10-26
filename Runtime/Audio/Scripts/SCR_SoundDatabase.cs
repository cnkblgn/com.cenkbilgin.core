using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    public static class SoundDatabase
    {
        public static int Version => version; private static int version;

        private static readonly Dictionary<string, int> idLookup = new();
        private static AudioClip[] clips = Array.Empty<AudioClip>();

        internal static void Build(AudioClip[] _clips)
        {
            if (_clips == null)
            {
                return;
            }

            List<AudioClip> validClips = new(_clips.Length);
            idLookup.Clear();

            for (int i = 0; i < _clips.Length; i++)
            {
                AudioClip clip = _clips[i];

                if (clip == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"Sound database clip is invalid at index {i}!");
#endif
                    continue;
                }

                string key = clip.name;

                idLookup[key] = validClips.Count;
                validClips.Add(clip);
            }

            clips = validClips.ToArray();

            version++;
            Debug.Log($"Sound database build successful! Clip count: {clips.Length}");
        }

        public static int GetClips() => clips.Length;
        public static int GetIDIndex(string key) => idLookup.TryGetValue(key, out int index) ? index : -1;
        public static SoundID GetID(int index)
        {
            if (index >= clips.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Sound id not found index out of range");
            }

            return new(clips[index].name, index);
        }
        public static AudioClip GetClip(int index)
        {
            if (index >= clips.Length || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"Sound not found index out of range");
            }

            return clips[index];
        }
        public static AudioClip GetClip(SoundID id)
        {
            if (!id.IsValid)
            {
                throw new ArgumentNullException(nameof(id), $"Sound id [{id}] is not valid!");
            }

            return GetClip(id.Index);
        }
    }
}