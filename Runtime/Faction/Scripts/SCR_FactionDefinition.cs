namespace Core.Faction
{
    public sealed class FactionDefinition
    {
        public readonly FactionID ID;
        internal readonly FactionRelation[] Relations;
        internal readonly FactionAction[] Actions;
        internal readonly Compatibility[] Compatibilities;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal FactionAttitude GetAttitude(FactionID target) => FactionDatabase.EvaluateAttitude(GetRelation(target));
        internal int GetRelation(FactionID target)
        {
            if (!target.IsValid || target.Index >= Relations.Length)
            {
                return 0;
            }

            return Relations[target.Index].Relation;
        }

        internal FactionDefinition(FactionID id, FactionRelation[] relations, FactionAction[] actions, Compatibility[] compatibilities)
        {
            ID = id;
            Relations = relations ?? FactionRelation.NONE;
            Actions = actions ?? FactionAction.NONE;
            Compatibilities = compatibilities ?? Compatibility.NONE;

            IconID = $"Faction{ID.Key}";
            NameID = $"faction.{ID.Key}Name";
            DescID = $"faction.{ID.Key}Desc";
        }
        internal FactionDefinition(FactionEntry entry) : this(entry.ID, entry.Relations, entry.Actions, entry.Compatibilities) { }
    }
}
