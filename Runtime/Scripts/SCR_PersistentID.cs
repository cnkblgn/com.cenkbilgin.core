using Newtonsoft.Json;
using System;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

namespace Core
{
    using static CoreUtility;

#if UNITY_EDITOR
    [DebuggerDisplay("{ToString()}")]
#endif
    public readonly struct PersistentID : IEquatable<PersistentID>
    {
        [JsonProperty("0")] public readonly string Prefix;
        [JsonProperty("1")] public readonly int Value;
        [JsonIgnore] private readonly string cachedString;

        public PersistentID(string prefix, int value)
        {
            Prefix = prefix ?? STRING_EMPTY;
            Value = value;

            cachedString = null;
        }
        public readonly override string ToString() => cachedString ?? $"{Prefix}_{Value}";
        public readonly bool Equals(PersistentID id) => Value == id.Value && Prefix == id.Prefix;
        public readonly override bool Equals(object obj) => obj is PersistentID other && Equals(other);
        public readonly override int GetHashCode() => HashCode.Combine(Prefix, Value);
        public static bool TryParse(string raw, out PersistentID id)
        {
            id = default;

            if (string.IsNullOrEmpty(raw))
            {
                return false;
            }

            int separator = raw.LastIndexOf('_');

            if (separator <= 0 || separator >= raw.Length - 1)
            {
                return false;
            }

            string prefix = raw[..separator];
            string number = raw[(separator + 1)..];

            if (!int.TryParse(number, out int value))
            {
                return false;
            }

            if (value < 0)
            {
                return false;
            }

            id = new(prefix, value);

            return true;
        }
    }
}
