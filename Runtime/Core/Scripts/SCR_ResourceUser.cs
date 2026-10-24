namespace Core
{
    public interface IResourceUser
    {
        public void HandleResourceStateChanged(in ResourceContext ctx);
    }
}