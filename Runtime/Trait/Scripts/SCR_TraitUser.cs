namespace Core.Trait
{
    public interface ITraitUser
    {
        public void HandleTraitStateChanged(in TraitContext ctx);
    }
}
