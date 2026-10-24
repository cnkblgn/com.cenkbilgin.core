namespace Core.Effect
{
    public interface IEffectUser
    {
        public void HandleEffectChanged(in EffectContext ctx);
    }
}
