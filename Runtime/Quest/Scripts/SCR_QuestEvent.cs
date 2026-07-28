namespace Core.Quest
{
    public enum QuestEvent : byte
    {
        NONE,
        INTERACT,
        PICKUP,
        USE,
        TRIGGER,
        ACTIVATE,
        DEACTIVATE,
        TALK,
        KILL,
        EXPLODE,
        SELL,
        BUY,
        DROP,
    }
}
