namespace Core.Effect
{
    public interface IEffectHandler
    {
        public void HandleEffectChanged(in EffectContext ctx);
    }
}
