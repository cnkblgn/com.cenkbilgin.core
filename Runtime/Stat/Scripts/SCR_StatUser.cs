namespace Core.Stat
{
    public interface IStatUser
    {
        public void HandleStatStateChanged(in StatContext ctx);
    }
}