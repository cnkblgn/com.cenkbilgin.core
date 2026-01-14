using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    public class PersistentIDGenerator
    {
        [JsonProperty("0")] public int NextID = 0;
        [JsonProperty("1")] public readonly string Prefix = STRING_EMPTY;

        [JsonConstructor] public PersistentIDGenerator([JsonProperty("0")] int nextID, [JsonProperty("1")] string prefix)
        {
            this.NextID = nextID;
            this.Prefix = prefix;
        }

        public PersistentID Generate() => new(Prefix, NextID++);
        public void Sync(IEnumerable<string> ids)
        {
            int max = NextID - 1;

            foreach (string raw in ids)
            {
                if (!PersistentID.TryParse(raw, out PersistentID id))
                {
                    continue;
                }

                if (id.Prefix != Prefix)
                {
                    continue;
                }

                if (id.Value > max)
                {
                    max = id.Value;
                }
            }

            NextID = max + 1;
        }
    }
}
