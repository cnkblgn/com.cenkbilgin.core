using System;

namespace Core.Input
{
    public readonly struct InputAction : IEquatable<InputAction>
    {
        internal readonly string path;

        public InputAction(string value) => path = value;
        public static implicit operator string(InputAction id) => id.path;
      
        public bool Equals(InputAction other) => path == other.path;
        public override bool Equals(object obj) => obj is InputAction other && Equals(other);

        public override int GetHashCode() => path != null ? path.GetHashCode() : 0;

        public override string ToString() => path;
    }
}
