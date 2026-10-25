namespace Core.Item
{
    public interface IInventoryHandler
    {
        public void HandleInventoryChanged(in InventoryContext ctx);
    }
}