namespace Core.Weapon
{
    public interface IWeaponModule 
    {
        public void Initialize(Weapon weapon);
        public void Tick(float deltaTime);
    }
}