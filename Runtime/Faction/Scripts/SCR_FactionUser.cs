namespace Core.Faction
{
    public interface IFactionUser
    {
        public void HandleFactionStateChanged(in FactionContext ctx);
    }
}
