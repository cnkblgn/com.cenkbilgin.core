using System;
using System.Collections.Generic;

namespace Core.Faction
{
    public readonly struct FactionContext
    {
        public readonly FactionID ID;
        public readonly IReadOnlyList<FactionRelation> Relations;

        internal FactionContext(FactionID id, IReadOnlyList<FactionRelation> relations)
        {
            ID = id;
            Relations = relations ?? throw new ArgumentNullException(nameof(relations), "Faction context ctor failed! relations cannot be null!");
        }
    }
}