using System;

namespace Core
{
    public readonly struct ActorEntry : IEquatable<ActorEntry>
    {
        public readonly int ID;
        public readonly Actor Actor;

        public ActorEntry(Actor actor)
        {
            Actor = actor != null ? actor : throw new ArgumentNullException(nameof(actor));
            ID = Actor.GetInstanceID();
        }

        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly bool Equals(ActorEntry other) => ID == other.ID;
        public readonly override bool Equals(object obj) => obj is ActorEntry other && Equals(other);
        public static bool operator ==(ActorEntry left, ActorEntry right) => left.Equals(right);
        public static bool operator !=(ActorEntry left, ActorEntry right) => !left.Equals(right);
    }
}
