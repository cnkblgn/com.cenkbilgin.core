using System;

namespace Core.UI
{
    public readonly struct UIPromptHandle : IEquatable<UIPromptHandle>
    {
        internal readonly Guid ID;
        internal readonly UIPromptView View;

        internal UIPromptHandle(Guid id, UIPromptView view)
        {
            ID = id;
            View = view != null ? view : throw new ArgumentNullException(nameof(view), "UI Prompt handle view reference is missing!?");
        }


        public bool IsValid()
        {
            if (View == null)
            {
                return false;
            }

            return View.IsActive;
        }
        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly override bool Equals(object obj) => obj is UIPromptHandle other && Equals(other);
        public readonly bool Equals(UIPromptHandle other) => ID == other.ID;
        public static bool operator ==(UIPromptHandle left, UIPromptHandle right) => left.Equals(right);
        public static bool operator !=(UIPromptHandle left, UIPromptHandle right) => !left.Equals(right);
    }
}