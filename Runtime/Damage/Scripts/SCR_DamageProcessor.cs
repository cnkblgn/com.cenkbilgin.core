namespace Core.Damage
{
    public interface IDamageProcessor
    {
        public bool HandleCanDamageTarget(in DamageData data);
        public void HandleAfterDamagedTarget(in DamageContext ctx);
    }
}
