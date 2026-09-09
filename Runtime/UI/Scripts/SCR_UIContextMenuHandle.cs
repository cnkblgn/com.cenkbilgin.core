using System;

namespace Core.UI
{
    public readonly struct UIContextMenuHandle : IEquatable<UIContextMenuHandle>
    {
        internal readonly Guid ID;
        internal readonly UIContextMenuController Controller;

        internal UIContextMenuHandle(Guid id, UIContextMenuController controller)
        {
            ID = id;
            Controller = controller != null ? controller : throw new ArgumentNullException(nameof(controller), "UI Context menu handle controller is missing!?");
        }

        public bool IsValid()
        {
            if (Controller == null)
            {
                return false;
            }

            return Controller.IsActive;
        }
        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly override bool Equals(object obj) => obj is UIContextMenuHandle other && Equals(other);
        public readonly bool Equals(UIContextMenuHandle other) => ID == other.ID;
        public static bool operator ==(UIContextMenuHandle left, UIContextMenuHandle right) => left.Equals(right);
        public static bool operator !=(UIContextMenuHandle left, UIContextMenuHandle right) => !left.Equals(right);
    }
}