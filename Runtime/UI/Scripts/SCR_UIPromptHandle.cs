using System;

namespace Core.UI
{
    public readonly struct UIPromptHandle : IEquatable<UIPromptHandle>
    {
        internal readonly Guid ID;
        internal UIPromptHandle(Guid id) => ID = id;

        public readonly override int GetHashCode() => ID.GetHashCode();
        public readonly override bool Equals(object obj) => obj is UIPromptHandle other && Equals(other);
        public readonly bool Equals(UIPromptHandle other) => ID == other.ID;
        public static bool operator ==(UIPromptHandle left, UIPromptHandle right) => left.Equals(right);
        public static bool operator !=(UIPromptHandle left, UIPromptHandle right) => !left.Equals(right);
    }
}