namespace Core
{
    public interface IResourceHandler
    {
        public void HandleResourceChanged(in ResourceContext ctx);
    }
}