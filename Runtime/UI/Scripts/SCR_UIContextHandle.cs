using System;

namespace Core.UI
{
    public readonly struct UIContextHandle : IEquatable<UIContextHandle>
    {
        internal readonly Guid ID;
        internal UIContextHandle(Guid id) => ID = id;

        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly override bool Equals(object obj) => obj is UIContextHandle other && Equals(other);
        public readonly bool Equals(UIContextHandle other) => ID == other.ID;
        public static bool operator ==(UIContextHandle left, UIContextHandle right) => left.Equals(right);
        public static bool operator !=(UIContextHandle left, UIContextHandle right) => !left.Equals(right);
    }
}