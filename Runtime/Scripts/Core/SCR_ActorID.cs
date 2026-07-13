using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public partial struct ActorID : IEquatable<ActorID>
    {
        public static readonly ActorID NONE = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;

        public ActorID(string key) => this.key = key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(ActorID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is ActorID other && Equals(other);
        public static bool operator ==(ActorID left, ActorID right) => left.Equals(right);
        public static bool operator !=(ActorID left, ActorID right) => !left.Equals(right);

        public readonly bool TryGetAnyActor(out Actor actor) => ActorDatabase.TryGetAnyActor(this, out actor);
        public readonly bool TryGetAllActors(out IReadOnlyList<ActorEntry> entries) => ActorDatabase.TryGetAllActors(this, out entries);
    }
}
