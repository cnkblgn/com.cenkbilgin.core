namespace Core
{
    public interface IResourceUser
    {
        public void HandleResourceChanged(in ResourceContext ctx);
    }
}