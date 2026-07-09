using System;
using UnityEngine;

namespace Core.Actor
{
    using static CoreUtility;

    [Serializable]
    public struct ActorTag : IEquatable<ActorTag>
    {
        public static readonly ActorTag Empty = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => 1UL << index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public ActorTag(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly bool Equals(ActorTag other) => key == other.key && index == other.index;
        public readonly override int GetHashCode() => HashCode.Combine(key, index);
    }
}