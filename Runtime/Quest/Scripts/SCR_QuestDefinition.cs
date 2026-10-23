namespace Core.Quest
{
    public sealed class QuestDefinition
    {
        public readonly QuestID ID;
        public readonly QuestID NextID;

        internal readonly QuestRequirement[] Requirements;
        internal readonly QuestCondition[] Conditions;
        internal readonly QuestAction[] Actions;

        public readonly string IconID;
        public readonly string NameID;
        public readonly string DescID;

        internal QuestDefinition(QuestID id, QuestID nextID, QuestRequirement[] requirements, QuestCondition[] conditions, QuestAction[] actions)
        {
            ID = id;
            NextID = nextID;
            Requirements = requirements ?? QuestRequirement.NONE;
            Conditions = conditions ?? QuestCondition.NONE;
            Actions = actions ?? QuestAction.NONE;

            IconID = $"Quest{ID.Key}";
            NameID = $"quest.{ID.Key}Name";
            DescID = $"quest.{ID.Key}Desc";
        }
        internal QuestDefinition(QuestID id, QuestID nextID) : this(id, nextID, default, default, default) { }
        internal QuestDefinition(QuestEntry entry) : this (entry.ID, entry.NextID, entry.Requirements, entry.Conditions, entry.Actions) { }
    }
}
