namespace Core.Item
{
    public readonly struct InventoryContext
    {
        public readonly InventoryState State;
        public readonly InventoryResult Result;
        public readonly InventoryEntity BaseInventory;
        public readonly InventoryEntity TargetInventory;
        public readonly ItemData Item;

        public InventoryContext(InventoryState state, InventoryResult result, InventoryEntity baseInventory, InventoryEntity target, ItemData item)
        {
            State = state;
            Result = result;
            BaseInventory = baseInventory;
            TargetInventory = target;
            Item = item;
        }
    }
}
