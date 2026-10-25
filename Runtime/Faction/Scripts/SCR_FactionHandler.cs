namespace Core.Faction
{
    public interface IFactionHandler
    {
        public void HandleFactionChanged(in FactionContext ctx);
    }
}
