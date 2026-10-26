using System;

namespace Core.Race
{
    public readonly struct RaceMask : IEquatable<RaceMask>
    {
        public static readonly RaceMask ALL = new(ulong.MaxValue);
        public static readonly RaceMask NONE = new(0UL);

        public bool IsValid => Value != 0;

        public readonly ulong Value;

        public RaceMask(ulong value) => Value = value;

        public bool HasAll(RaceMask target) => (Value & target.Value) == target.Value;
        public bool HasAll(RaceTag[] target) => HasAll(target.CreateMask());
        public bool HasAny(RaceMask target) => (Value & target.Value) != 0;
        public bool HasAny(RaceTag target) => HasAny(target.Mask);
        public bool HasAny(RaceTag[] target) => HasAny(target.CreateMask());
        public bool Equals(RaceMask other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RaceMask other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static RaceMask operator ~(RaceMask mask) => new(~mask.Value);
        public static bool operator ==(RaceMask left, RaceMask right) => left.Equals(right);
        public static bool operator !=(RaceMask left, RaceMask right) => !left.Equals(right);
    }
}
