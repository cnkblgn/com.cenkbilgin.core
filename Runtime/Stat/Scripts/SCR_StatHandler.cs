namespace Core.Stat
{
    public interface IStatHandler
    {
        public void HandleStatChanged(in StatContext ctx);
    }
}