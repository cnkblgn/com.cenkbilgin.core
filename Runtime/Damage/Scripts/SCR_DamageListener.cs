namespace Core.Damage
{
    public interface IDamageListener
    {
        public void HandleDamage(in DamageContext ctx);
    }
}
