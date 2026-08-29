namespace Core
{
    public interface IHitListener
    {
        public void HandleHit(in HitData data);
    }
}