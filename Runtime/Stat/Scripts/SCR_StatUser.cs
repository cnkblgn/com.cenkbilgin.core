namespace Core.Stat
{
    public interface IStatUser
    {
        public void HandleStatChanged(in StatContext ctx);
    }
}