using System;
using UnityEngine;

namespace Core.Audio
{
    using static CoreUtility;

    [Serializable]
    public partial struct SoundID : IEquatable<SoundID> 
    {
        public static readonly SoundID Empty = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;

        public SoundID(string key) => this.key = key;
        public readonly bool Equals(SoundID other) => key == other.key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
    }
}
