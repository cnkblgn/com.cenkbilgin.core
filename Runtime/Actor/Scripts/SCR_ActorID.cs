using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Actors
{
    [Serializable]
    public partial struct ActorID : IEquatable<ActorID>
    {
        public readonly string Key => key;
        public readonly int Index => index;
        public readonly bool IsValid => !string.IsNullOrEmpty(key) && index >= 0;

        [SerializeField, Required] private string key;
        [SerializeField, ReadOnly] private int index;

        public ActorID(string key, int index)
        {
            this.key = key;
            this.index = index;
        }

        public readonly override string ToString() => $"Key: {key} << Index: {index}";
        public readonly override int GetHashCode() => index;
        public readonly override bool Equals(object obj) => obj is ActorID other && Equals(other);
        public readonly bool Equals(ActorID other) => index == other.index;
        public static bool operator ==(ActorID left, ActorID right) => left.Equals(right);
        public static bool operator !=(ActorID left, ActorID right) => !left.Equals(right);

        public readonly bool TryGetAnyActor(out Actor actor) => ActorDatabase.TryGetAnyActor(this, out actor);
        public readonly bool TryGetAllActors(out IReadOnlyList<ActorEntry> entries) => ActorDatabase.TryGetAllActors(this, out entries);
    }
}
