using UnityEngine;

namespace Core.Audio
{
    public readonly struct SoundDefinition
    {
        public readonly SoundID ID;
        public readonly AudioClip Clip;

        public SoundDefinition(SoundID id, AudioClip clip)
        {
            ID = id;
            Clip = clip;
        }
    }
}