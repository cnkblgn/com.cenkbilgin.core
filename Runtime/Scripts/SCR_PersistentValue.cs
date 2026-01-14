using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public struct PersistentValue
    {
        [JsonProperty("f", NullValueHandling = NullValueHandling.Ignore)] public float? Float;
        [JsonProperty("i", NullValueHandling = NullValueHandling.Ignore)] public int? Int;
        [JsonProperty("b", NullValueHandling = NullValueHandling.Ignore)] public bool? Bool;
        [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)] public string String;
        [JsonProperty("v3", NullValueHandling = NullValueHandling.Ignore)] public Float3? Vector3;
        [JsonProperty("v2", NullValueHandling = NullValueHandling.Ignore)] public Float2? Vector2;
        [JsonProperty("g", NullValueHandling = NullValueHandling.Ignore)] public Guid? Guid;
        [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)] public Dictionary<string, PersistentValue> Data;
    }
}
