using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    [Serializable]
    public struct ActorTag : IEquatable<ActorTag>
    {
        public readonly string Key => key;

        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = ActorDatabase.GetTagIndex(key);
                }

                return index;
            }
        }
        public ulong Mask
        {
            get
            {
                int value = Index;

                return value >= 0 && value < 64 ? 1UL << value : 0;
            }
        }
        public bool IsValid
        {
            get
            {
                int value = Index;

                return !string.IsNullOrEmpty(key) && value >= 0 && value < 64;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public ActorTag(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly bool Equals(ActorTag other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public readonly override bool Equals(object obj) => obj is ActorTag other && Equals(other);
        public static bool operator ==(ActorTag left, ActorTag right) => left.Equals(right);
        public static bool operator !=(ActorTag left, ActorTag right) => !left.Equals(right);

        public static ulong CreateMask(ActorTag[] tags)
        {
            ulong mask = 0;

            if (tags == null)
            {
                return mask;
            }

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