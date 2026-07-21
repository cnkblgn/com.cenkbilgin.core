using System;
using UnityEngine;

namespace Core
{
    public readonly struct SearchKey : IEquatable<SearchKey>
    {
        public readonly EntityId ID;
        public readonly string Path;

        public SearchKey(EntityId id, string path)
        {
            ID = id;
            Path = path;
        }

        public override int GetHashCode() => HashCode.Combine(ID, Path);

        public bool Equals(SearchKey other) => ID.Equals(other.ID) && string.Equals(Path, other.Path, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is SearchKey other && Equals(other);
        public static bool operator ==(SearchKey left, SearchKey right) => left.Equals(right);
        public static bool operator !=(SearchKey left, SearchKey right) => !left.Equals(right);
    }
}
