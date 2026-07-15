using System;
using System.Collections.Generic;

namespace Core
{
    [Serializable]
    public sealed class PersistentEntityData
    {
        public readonly PrefabID PrefabID;
        public readonly Guid InstanceID;
        public bool IsMarkedForDestroy;
        public readonly Dictionary<string, DataNode> Data;

        public PersistentEntityData(PrefabID prefabID, Guid instanceID, bool isMarkedForDestroy, Dictionary<string, DataNode> data)
        {
            PrefabID = prefabID;
            InstanceID = instanceID;
            IsMarkedForDestroy = isMarkedForDestroy;
            Data = data == null ? new() : new(data);
        }
        public PersistentEntityData() : this(PrefabID.NONE, Guid.Empty, false, new()) { }
    }
}
