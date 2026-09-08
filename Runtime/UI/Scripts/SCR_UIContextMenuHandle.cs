using System;

namespace Core.UI
{
    public readonly struct UIContextMenuHandle : IEquatable<UIContextMenuHandle>
    {
        internal readonly Guid ID;
        internal UIContextMenuHandle(Guid id) => ID = id;

        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly override bool Equals(object obj) => obj is UIContextMenuHandle other && Equals(other);
        public readonly bool Equals(UIContextMenuHandle other) => ID == other.ID;
        public static bool operator ==(UIContextMenuHandle left, UIContextMenuHandle right) => left.Equals(right);
        public static bool operator !=(UIContextMenuHandle left, UIContextMenuHandle right) => !left.Equals(right);
    }
}