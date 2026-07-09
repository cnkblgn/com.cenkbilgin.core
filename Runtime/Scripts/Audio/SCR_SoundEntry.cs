using System;
using UnityEngine;

namespace Core.Audio
{
    internal readonly struct SoundEntry
    {
        public readonly AudioClip Clip;
        public readonly int Index;

        public SoundEntry(AudioClip clip, int index)
        {
            Clip = clip != null ? clip : throw new ArgumentNullException(nameof(clip));
            Index = index;
        }
    }
}