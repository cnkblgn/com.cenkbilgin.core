using System;
using System.Collections.Generic;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public sealed class PersistentEntityData
    {
        public readonly string TypeID;
        public readonly string PrefabID;
        public readonly Guid InstanceID;
        public bool IsMarkedForDestroy;
        public readonly Dictionary<string, DataNode> Data;

        public PersistentEntityData(string typeID, string prefabID, Guid instanceID, bool isMarkedForDestroy, Dictionary<string, DataNode> data)
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
