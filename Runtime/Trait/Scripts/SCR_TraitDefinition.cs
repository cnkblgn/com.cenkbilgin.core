using System;
using Core.Actors;
using Core.Localization;
using Core.Graphics;

namespace Core.Trait
{
    public sealed class TraitDefinition
    {
        public readonly TraitID ID;
        public readonly TraitID[] IncompatibleIDs;
        public readonly IconID IconID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;

        public readonly int Cost;
        public readonly TraitAction Action;

        public void Apply(Actor character, ref TraitInstance instance) => Action.Apply(character, ref instance);
        public void Remove(Actor character, ref TraitInstance instance) => Action.Remove(character, ref instance);

        private TraitDefinition(TraitID id, TraitID[] incompatibleIDs, IconID iconID, LocalizedID nameID, LocalizedID descID, int cost, TraitAction action)
        {
            ID = id;
            IncompatibleIDs = incompatibleIDs ?? (new TraitID[] { });
            Action = Action = action ?? throw new ArgumentNullException(nameof(action), "Trait action cannot be null! please assign!");
            Cost = cost;
            IconID = iconID;
            NameID = nameID;
            DescID = descID;
        }
        internal TraitDefinition(TraitEntry entry) : this(entry.ID, entry.IncompatibleIDs, entry.IconID, entry.NameID, entry.DescID, entry.Cost, entry.Action) { }
    }
}
