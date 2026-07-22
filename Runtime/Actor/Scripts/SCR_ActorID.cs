using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    using static CoreUtility;

    [Serializable]
    public partial struct ActorID : IEquatable<ActorID>
    {
        public static readonly ActorID NONE = new(STRING_EMPTY, 0);

        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key);

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public ActorID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }
        public readonly override int GetHashCode() => key != null ? key.GetHashCode() : 0;
        public readonly override bool Equals(object obj) => obj is ActorID other && Equals(other);
        public readonly bool Equals(ActorID other) => key == other.key;
        public static bool operator ==(ActorID left, ActorID right) => left.Equals(right);
        public static bool operator !=(ActorID left, ActorID right) => !left.Equals(right);

        public readonly bool TryGetAnyActor(out Actor actor) => ActorDatabase.TryGetAnyActor(this, out actor);
        public readonly bool TryGetAllActors(out IReadOnlyList<ActorEntry> entries) => ActorDatabase.TryGetAllActors(this, out entries);
    }
}
