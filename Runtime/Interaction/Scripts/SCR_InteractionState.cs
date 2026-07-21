namespace Core.Interaction
{
    public enum InteractionState : byte
    {
        DEFAULT,
        INTERACT_ACCEPTED,
        INTERACT_DENIED,
        FOCUS_ENTER,
        FOCUS_EXIT,
    }
}