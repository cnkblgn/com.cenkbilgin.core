using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    [Serializable]
    public struct ActorID : IEquatable<ActorID>
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

                index = ActorDatabase.GetIDIndex(key);

                resolved = index >= 0;

                return index;
            }
        }

        [SerializeField, Required] private string key;
        [NonSerialized] private int index;
        [NonSerialized] private bool resolved;

        public ActorID(string key, int index) => (this.key, this.index, this.resolved) = (key, index, index >= 0);
        public ActorID(string key) : this(key, -1) { }

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
