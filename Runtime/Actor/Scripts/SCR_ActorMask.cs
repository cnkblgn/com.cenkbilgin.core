using System;

namespace Core.Actors
{
    public readonly struct ActorMask : IEquatable<ActorMask>
    {
        public bool IsValid => Value != 0;

        public readonly ulong Value;

        public ActorMask(ulong value) => Value = value;

        public bool HasAll(ActorMask target) => (Value & target.Value) == target.Value;
        public bool HasAll(ActorTag[] target) => HasAll(target.CreateMask());
        public bool HasAny(ActorMask target) => (Value & target.Value) != 0;
        public bool HasAny(ActorTag target) => HasAny(target.Mask);
        public bool HasAny(ActorTag[] target) => HasAny(target.CreateMask());
        public bool Equals(ActorMask other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ActorMask other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(ActorMask left, ActorMask right) => left.Equals(right);
        public static bool operator !=(ActorMask left, ActorMask right) => !left.Equals(right);
    }
}
