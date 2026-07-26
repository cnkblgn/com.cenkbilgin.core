namespace Core.Item
{
    public enum InventoryResult : byte
    {
        NULL,
        DUPLICATE,
        OVERLAPPING,
        NOT_REGISTERED,
        NOT_SUPPORTED,
        NO_VALID_SPACE,
        OUT_OF_BOUNDS,
        WEIGHT_LIMIT_EXCEEDED,
        CAPACITY_LIMIT_EXCEEDED,
        SUCCESS,
    }
}
