namespace Core.Item
{
    internal interface IInventoryUser
    {
        public void HandleInventoryChanged(InventoryContext ctx);
    }
}
