namespace Core.Item
{
    public enum InventoryState : byte
    {
        DEFAULT,
        INITIALIZED,
        SORTED,
        ITEM_ADDED,
        ITEM_REMOVED,
        ITEM_DROPPED,
        ITEM_TRANSFERED,
        ITEM_CHANGED,
    }
}