using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public class PersistentEntityData
    {
        [JsonProperty("0")] public readonly string TypeID = STRING_EMPTY;
        [JsonProperty("1")] public readonly string PrefabID = STRING_EMPTY;
        [JsonProperty("2")] public readonly Guid InstanceID = Guid.Empty;
        [JsonProperty("3")] public bool IsMarkedForDestroy = false;
        [JsonProperty("4")] public readonly Dictionary<string, PersistentValue> Data = new();

        [JsonConstructor] public PersistentEntityData([JsonProperty("0")] string typeID, [JsonProperty("1")] string prefabID, [JsonProperty("2")] Guid instanceID, [JsonProperty("3")] bool isMarkedForDestroy, [JsonProperty("4")] Dictionary<string, PersistentValue> data)
        {
            TypeID = typeID;
            PrefabID = prefabID;
            InstanceID = instanceID;
            IsMarkedForDestroy = isMarkedForDestroy;
            Data = data == null ? new() : new(data);
        }
        public PersistentEntityData() : this(STRING_EMPTY, STRING_EMPTY, Guid.Empty, false, new()) { }
    }
}
