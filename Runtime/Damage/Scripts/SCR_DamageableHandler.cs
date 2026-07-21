namespace Core.Damage
{
    public interface IDamageableHandler
    {
        /// <summary> Called when resolving damage. Use this for handling incoming damage. </summary>
        public float HandleDamage(in DamageData data);
        public void HandleHit(in DamageContext ctx);
    }
}
