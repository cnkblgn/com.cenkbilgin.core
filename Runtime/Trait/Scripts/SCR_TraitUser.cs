namespace Core.Trait
{
    public interface ITraitUser
    {
        public void HandleTraitChanged(in TraitContext ctx);
    }
}
