namespace Core.Item
{
    public interface IInventoryUser
    {
        public void HandleStateChanged(InventoryContext ctx);
    }
}
