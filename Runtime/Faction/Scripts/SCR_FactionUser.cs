namespace Core.Faction
{
    public interface IFactionUser
    {
        public void HandleFactionChanged(in FactionContext ctx);
    }
}
