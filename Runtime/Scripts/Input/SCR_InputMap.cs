using System;

namespace Core.Input
{
    using static CoreUtility;

    public readonly struct InputMap : IEquatable<InputMap>
    {
        internal readonly string name;

        public InputMap(string value) => name = value ?? STRING_NULL;
        public static implicit operator string(InputMap map) => map.name;

        public bool Equals(InputMap other) => name == other.name;
        public override bool Equals(object obj) => obj is InputMap other && Equals(other);

        public override int GetHashCode() => name != null ? name.GetHashCode() : 0;

        public override string ToString() => name;
    }
}
