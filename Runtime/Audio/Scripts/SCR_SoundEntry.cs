using System;
using UnityEngine;

namespace Core.Audio
{
    internal readonly struct SoundEntry : IEquatable<SoundEntry>
    {
        public readonly SoundID ID;
        public readonly AudioClip Clip;

        public SoundEntry(SoundID id, AudioClip clip)
        {
            ID = id;
            Clip = clip != null ? clip : throw new ArgumentNullException($"Sound entry clip is missing! {nameof(clip)}");
        }

        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly bool Equals(SoundEntry other) => ID == other.ID;
        public readonly override bool Equals(object obj) => obj is SoundEntry other && Equals(other);
        public static bool operator ==(SoundEntry left, SoundEntry right) => left.Equals(right);
        public static bool operator !=(SoundEntry left, SoundEntry right) => !left.Equals(right);
    }
}