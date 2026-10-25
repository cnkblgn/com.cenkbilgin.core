using System;

namespace Core.Damage
{
    public readonly struct DamageMask : IEquatable<DamageMask>
    {
        public bool IsValid => Value != 0;

        public readonly ulong Value;

        public DamageMask(ulong value) => Value = value;

        public bool HasAll(DamageMask target) => (Value & target.Value) == target.Value;
        public bool HasAll(DamageTag[] target) => HasAll(target.CreateMask());
        public bool HasAny(DamageMask target) => (Value & target.Value) != 0;
        public bool HasAny(DamageTag target) => HasAny(target.Mask);
        public bool HasAny(DamageTag[] target) => HasAny(target.CreateMask());
        public bool Equals(DamageMask other) => Value == other.Value;
        public override bool Equals(object obj) => obj is DamageMask other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(DamageMask left, DamageMask right) => left.Equals(right);
        public static bool operator !=(DamageMask left, DamageMask right) => !left.Equals(right);
    }
}
