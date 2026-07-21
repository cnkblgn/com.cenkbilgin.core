using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public sealed class SearchState<T>
    {
        public bool Open;
        public bool Dirty = true;
        public bool Focus;

        public string Search = string.Empty;
        public Vector2 Scroll;

        public SearchCollection<T> Cached;
        public readonly List<SearchEntry<T>> Filtered = new(64);
    }
}
