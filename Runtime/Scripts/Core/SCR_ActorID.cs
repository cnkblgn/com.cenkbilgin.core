using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public partial struct ActorID : IEquatable<ActorID>
    {
        public static readonly ActorID Empty = new(STRING_EMPTY);

        public readonly string Key => key;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField] private string key;

        public ActorID(string key) => this.key = key;
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly bool Equals(ActorID other) => key == other.key;
        public readonly override bool Equals(object obj) => obj is ActorID other && Equals(other);
        public static bool operator ==(ActorID left, ActorID right) => left.Equals(right);
        public static bool operator !=(ActorID left, ActorID right) => !left.Equals(right);

        public readonly bool TryGetActor(out Actor actor) => ActorDatabase.TryGetActor(this, out actor);
    }
}
