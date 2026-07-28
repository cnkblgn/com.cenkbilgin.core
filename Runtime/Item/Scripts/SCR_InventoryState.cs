namespace Core.Item
{
    public enum InventoryState : byte
    {
        DEFAULT,
        INITIALIZED,
        ITEM_ADDED,
        ITEM_REMOVED,
        ITEM_DROPPED,
        ITEM_TRANSFERED,
        OPENED,
        CLOSED,
    }
}