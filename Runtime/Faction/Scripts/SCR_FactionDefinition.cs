namespace Core.Faction
{
    public sealed class FactionDefinition
    {
        public readonly FactionID ID;
        internal readonly FactionValidator[] Validators;
        internal readonly FactionRelation[] Relations;

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

        internal FactionDefinition(FactionID id, FactionValidator[] validators, FactionRelation[] relations)
        {
            ID = id;
            IconID = $"Icon{ID.Key}";
            NameID = $"faction.{ID.Key}.name";
            DescID = $"faction.{ID.Key}.desc";

            Validators = validators ?? FactionValidator.NONE;
            Relations = relations ?? FactionRelation.NONE;
        }
        internal FactionDefinition(FactionEntry entry) : this(entry.ID, entry.Validators, entry.Relations) { }
    }
}
