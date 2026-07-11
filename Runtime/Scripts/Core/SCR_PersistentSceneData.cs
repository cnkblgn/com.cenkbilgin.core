using System;
using System.Collections.Generic;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public sealed class PersistentSceneData
    {
        public string ID;
        public Dictionary<Guid, PersistentEntityData> AvailableEntities;
        public HashSet<Guid> DeletedEntities;

        public PersistentSceneData(string id)
        {
            ID = id ?? STRING_NULL;
            AvailableEntities = new();
            DeletedEntities = new();
        }
        public PersistentSceneData() : this(STRING_NULL) { }
    }
}