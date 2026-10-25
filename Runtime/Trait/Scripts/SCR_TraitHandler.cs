namespace Core.Trait
{
    public interface ITraitHandler
    {
        public void HandleTraitChanged(in TraitContext ctx);
    }
}
