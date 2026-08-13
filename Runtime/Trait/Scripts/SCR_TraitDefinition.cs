using Core.Localization;
using Core.Graphics;

namespace Core.Trait
{
    public sealed class TraitDefinition
    {
        public readonly TraitID ID;
        internal readonly TraitID[] IncompatibleIDs;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;
        internal readonly TraitAction[] Actions;
        public readonly int Cost;

        internal TraitDefinition(TraitID id, TraitID[] incompatibleIDs, IconID iconID, LocalizedID nameID, LocalizedID descID, int cost, TraitAction[] actions)
        {
            ID = id;
            IncompatibleIDs = incompatibleIDs ?? (new TraitID[] { });
            Actions = actions ?? (new TraitAction[] { new TraitActionNone() });
            Cost = cost;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
        }
        internal TraitDefinition(TraitEntry entry) : this(entry.ID, entry.IncompatibleIDs, entry.IconID, entry.NameID, entry.DescID, entry.Cost, entry.Actions) { }
    }
}