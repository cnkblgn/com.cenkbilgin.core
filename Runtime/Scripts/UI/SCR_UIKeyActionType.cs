using System;

namespace Core.UI
{
    public readonly struct UIKeyActionType : IEquatable<UIKeyActionType>
    {
        public readonly string Value;

        public UIKeyActionType(string value) => Value = value;
        public static implicit operator string(UIKeyActionType id) => id.Value;

        public bool Equals(UIKeyActionType other) => Value == other.Value;
        public override bool Equals(object obj) => obj is UIKeyActionType other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;
    }
}