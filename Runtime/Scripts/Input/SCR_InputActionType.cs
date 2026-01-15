using System;

namespace Core.Input
{
    public readonly struct InputActionType : IEquatable<InputActionType>
    {
        public readonly string Value;

        public InputActionType(string value) => Value = value;
        public static implicit operator string(InputActionType id) => id.Value;
      
        public bool Equals(InputActionType other) => Value == other.Value;
        public override bool Equals(object obj) => obj is InputActionType other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public override string ToString() => Value;
    }
}
