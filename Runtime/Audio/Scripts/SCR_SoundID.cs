using System;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [Serializable]
    public partial struct SoundID : IEquatable<SoundID> 
    {
        public static readonly SoundID NONE = new(STRING_EMPTY, -1);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public SoundID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => HashCode.Combine(key, index);
        public readonly bool Equals(SoundID other) => key == other.key && index == other.index;
        public readonly override bool Equals(object obj) => obj is SoundID other && Equals(other);
        public static bool operator ==(SoundID left, SoundID right) => left.Equals(right);
        public static bool operator !=(SoundID left, SoundID right) => !left.Equals(right);

        public readonly AudioClip GetClip() => SoundDatabase.GetClip(this);
        public readonly void Play(AudioGroup group) => ManagerAudio.Instance.PlaySound(GetClip(), group, Vector3.zero, 0, 1, 1, 1, 10, false);
        public readonly void Play(AudioGroup group, Vector3 position, float blend = 0, float volume = 1, float pitch = 1, float minDistance = 1, float maxDistance = 10, bool occulusion = false) => ManagerAudio.Instance.PlaySound(GetClip(), group, position, blend, volume, pitch, minDistance, maxDistance, occulusion);
    }
}
