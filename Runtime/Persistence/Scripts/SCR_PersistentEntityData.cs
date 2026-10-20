using System;
using System.Collections.Generic;

namespace Core.Persistence
{
    public sealed class PersistentEntityData
    {
        public readonly string PrefabID;
        public readonly Guid InstanceID;

        public bool IsMarkedForDestroy;

        public readonly Dictionary<string, DataNode> Data;

        public PersistentEntityData(string prefabID, Guid instanceID, bool isMarkedForDestroy, Dictionary<string, DataNode> data)
        {
            PrefabID = prefabID;
            InstanceID = instanceID;
            IsMarkedForDestroy = isMarkedForDestroy;
            Data = data == null ? new() : new(data);
        }
        public PersistentEntityData() : this(default, Guid.Empty, false, new()) { }
    }
}
