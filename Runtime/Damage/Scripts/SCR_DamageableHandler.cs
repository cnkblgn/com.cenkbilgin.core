namespace Core.Damage
{
    public interface IDamageableHandler
    {
        /// <summary> Called once when resolving damage. Use this for handling incoming damage. </summary>
        public float HandleDamage(in DamageData data);
        /// <summary> Called every time entity get hit. </summary>
        public void HandleHit(in DamageContext ctx);
    }
}
