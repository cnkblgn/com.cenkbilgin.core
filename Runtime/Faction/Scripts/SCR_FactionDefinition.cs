using Core.Graphics;
using Core.Localization;

namespace Core.Faction
{
    public sealed class FactionDefinition
    {
        public readonly FactionID ID;
        internal readonly FactionRelation[] Relations;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;

        internal FactionAttitude GetAttitude(FactionID target) => FactionDatabase.EvaluateAttitude(GetRelation(target));
        internal int GetRelation(FactionID target)
        {
            if (!target.IsValid || target.Index >= Relations.Length)
            {
                return 0;
            }

            return Relations[target.Index].Relation;
        }

        internal FactionDefinition(FactionID id, FactionRelation[] relations, IconID iconID, LocalizedID nameID, LocalizedID descID)
        {
            ID = id;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;

            Relations = relations ?? (new FactionRelation[0] { });
        }
        internal FactionDefinition(FactionEntry entry) : this(entry.ID, entry.Relations, entry.IconID, entry.NameID, entry.DescID) { }
    }
}
