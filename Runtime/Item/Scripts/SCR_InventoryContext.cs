namespace Core.Item
{
    public readonly struct InventoryContext
    {
        public readonly InventoryState State;
        public readonly InventoryResult Result;
        public readonly InventoryEntity Inventory;
        public readonly ItemData Item;

        public InventoryContext(InventoryState state, InventoryResult result, InventoryEntity inventory, ItemData item)
        {
            State = state;
            Result = result;
            Inventory = inventory;
            Item = item;
        }
    }
}
