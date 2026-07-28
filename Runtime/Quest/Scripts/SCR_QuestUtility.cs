namespace Core.Quest
{
    public static class QuestUtility
    {
        public static QuestDefinition GetDefinition(this QuestID id) => QuestDatabase.GetDefinition(id);
        public static QuestInstance CreateInstance(this QuestID id) => QuestDatabase.CreateInstance(id);
    }
}