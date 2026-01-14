using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public class PersistentSceneData
    {
        [Header("_")]
        [JsonProperty("0")] public string ID = STRING_NULL;
        [JsonProperty("1")] public Dictionary<Guid, PersistentInstanceData> Database = new();

        [JsonConstructor] public PersistentSceneData([JsonProperty("0")] string id, [JsonProperty("1")] Dictionary<Guid, PersistentInstanceData> database)
        {
            ID = id ?? STRING_NULL;
            Database = new();

            if (database != null)
            {
                foreach (var item in database)
                {
                    Database[item.Key] = new(item.Value.TypeID, item.Value.PrefabID, item.Value.InstanceID, item.Value.IsMarkedForDestroy, item.Value.Data);
                }
            }
        }
        public PersistentSceneData()
        {
            ID = STRING_NULL;
            Database = new();
        }      
    }
}