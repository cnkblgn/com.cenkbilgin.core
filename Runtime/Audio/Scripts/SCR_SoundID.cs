using System;
using UnityEngine;

namespace Core.Audio
{
    [Serializable]
    public struct SoundID : IEquatable<SoundID> 
    {
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;
        public int Index
        {
            get
            {
                if (resolved)
                {
                    return index;
                }

                index = SoundDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public SoundID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public SoundID(string key) : this(key, -1) { }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is SoundID other && Equals(other);
        public readonly bool Equals(SoundID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(SoundID left, SoundID right) => left.Equals(right);
        public static bool operator !=(SoundID left, SoundID right) => !left.Equals(right);

        public readonly AudioClip GetClip() => SoundDatabase.GetClip(this);
        public readonly void Play(AudioGroup group) => ManagerAudio.Instance.PlaySound(GetClip(), group, Vector3.zero, 0, 1, 1, 1, 10, false);
        public readonly void Play(AudioGroup group, Vector3 position, float blend = 0, float volume = 1, float pitch = 1, float minDistance = 1, float maxDistance = 10, bool occulusion = false) => ManagerAudio.Instance.PlaySound(GetClip(), group, position, blend, volume, pitch, minDistance, maxDistance, occulusion);
    }
}
