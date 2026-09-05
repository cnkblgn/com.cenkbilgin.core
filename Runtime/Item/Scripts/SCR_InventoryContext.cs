namespace Core.Item
{
    public readonly struct InventoryContext
    {
        public readonly InventoryState State;
        public readonly InventoryResult Result;
        public readonly ItemData Item;

        public InventoryContext(InventoryState state, InventoryResult result, ItemData item)
        {
            State = state;
            Result = result;
            Item = item;
        }
    }
}
