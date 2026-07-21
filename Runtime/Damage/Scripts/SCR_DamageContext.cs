namespace Core.Damage
{
    public struct DamageContext
    {
        public readonly DamageData Data;
        public DamageState State;
        public float Damage;

        public DamageContext(DamageData data)
        {
            Data = data;
            State = DamageState.HIT;
            Damage = data.Damage;
        }
    }
}
