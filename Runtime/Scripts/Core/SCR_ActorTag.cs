using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public partial struct ActorTag : IEquatable<ActorTag>
    {
        public static readonly ActorTag NONE = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly ulong Mask => IsValid ? 1UL << index : 0;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;
        [SerializeField, ReadOnly] private int index;

        public ActorTag(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => HashCode.Combine(key, index);
        public readonly bool Equals(ActorTag other) => key == other.key && index == other.index;
        public readonly override bool Equals(object obj) => obj is ActorTag other && Equals(other);
        public static bool operator ==(ActorTag left, ActorTag right) => left.Equals(right);
        public static bool operator !=(ActorTag left, ActorTag right) => !left.Equals(right);

        public static ulong CreateMask(ActorTag[] tags)
        {
            ulong mask = 0;

            for (int i = 0; i < tags.Length; i++)
            {
                mask |= tags[i].Mask;
            }

            return mask;
        }
        public readonly bool TryGetAnyActor(out Actor actor) => ActorDatabase.TryGetAnyActor(this, out actor);
        public readonly bool TryGetAllActors(out List<Actor> actors) => ActorDatabase.TryGetAllActors(this, out actors);
    }
}