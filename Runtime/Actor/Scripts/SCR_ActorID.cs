using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    [Serializable]
    public struct ActorID : IEquatable<ActorID>
    {
        public int Index
        {
            get
            {
                if (index < 0)
                {
                    index = ActorDatabase.GetIDIndex(key);
                }

                return index;
            }
        }
        public readonly string Key => key;
        public bool IsValid => !string.IsNullOrEmpty(key) && Index >= 0;

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;

        public ActorID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public override string ToString() => $"Key: {key} << Index: {Index}";

        public readonly override int GetHashCode() => key?.GetHashCode() ?? 0;
        public readonly override bool Equals(object obj) => obj is ActorID other && Equals(other);
        public readonly bool Equals(ActorID other) => string.Equals(key, other.key, StringComparison.Ordinal);
        public static bool operator ==(ActorID left, ActorID right) => left.Equals(right);
        public static bool operator !=(ActorID left, ActorID right) => !left.Equals(right);

        public readonly bool TryGetAnyActor(out Actor actor) => ActorDatabase.TryGetAnyActor(this, out actor);
        public readonly bool TryGetAllActors(out IReadOnlyList<ActorEntry> entries) => ActorDatabase.TryGetAllActors(this, out entries);
    }
}
