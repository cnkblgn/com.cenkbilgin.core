using System;

namespace Core.Surface
{
    public readonly struct SurfaceMask : IEquatable<SurfaceMask>
    {
        public bool IsValid => Value != 0;

        public readonly ulong Value;

        public SurfaceMask(ulong value) => Value = value;

        public bool HasAll(SurfaceMask target) => (Value & target.Value) == target.Value;
        public bool HasAll(SurfaceTag[] target) => HasAll(target.CreateMask());
        public bool HasAny(SurfaceMask target) => (Value & target.Value) != 0;
        public bool HasAny(SurfaceTag target) => HasAny(target.Mask);
        public bool HasAny(SurfaceTag[] target) => HasAny(target.CreateMask());
        public bool Equals(SurfaceMask other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SurfaceMask other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(SurfaceMask left, SurfaceMask right) => left.Equals(right);
        public static bool operator !=(SurfaceMask left, SurfaceMask right) => !left.Equals(right);
    }
}
