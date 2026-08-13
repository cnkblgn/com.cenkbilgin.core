using Core.Localization;

namespace Core.Quest
{
    public sealed class QuestDefinition
    {
        public readonly QuestID ID;
        public readonly QuestID NextID;
        public readonly LocalizedID NameID;
        public readonly LocalizedID DescID;
        internal readonly QuestRequirement[] Requirements;
        internal readonly QuestCondition[] Conditions;
        internal readonly QuestAction[] Actions;

        public QuestDefinition(QuestID id, QuestID nextID, LocalizedID nameID, LocalizedID descID, QuestRequirement[] requirements, QuestCondition[] conditions, QuestAction[] actions)
        {
            ID = id;
            NextID = nextID;
            Requirements = requirements ?? (new QuestRequirement[0]);
            Conditions = conditions ?? (new QuestCondition[0]);
            Actions = actions ?? (new QuestAction[0]);
            NameID = nameID;
            DescID = descID;
        }
        public QuestDefinition(QuestID id, QuestID nextID, LocalizedID nameID, LocalizedID descID) : this(id, nextID, nameID, descID, default, default, default) { }
        public QuestDefinition(QuestEntry entry) : this (entry.ID, entry.NextID, entry.NameID, entry.DescID, entry.Requirements, entry.Conditions, entry.Actions) { }
    }
}
