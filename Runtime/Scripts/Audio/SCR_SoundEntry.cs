using System;
using UnityEngine;

namespace Core.Audio
{
    internal readonly struct SoundEntry
    {
        public readonly SoundID ID;
        public readonly AudioClip Clip;

        public SoundEntry(SoundID iD, AudioClip clip)
        {
            ID = iD;
            Clip = clip ?? throw new ArgumentNullException(nameof(clip));
        }
    }
}