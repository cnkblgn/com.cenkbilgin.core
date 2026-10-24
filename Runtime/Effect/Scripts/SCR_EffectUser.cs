namespace Core.Effect
{
    public interface IEffectUser
    {
        public void HandleEffectStateChanged(in EffectContext ctx);
    }
}
