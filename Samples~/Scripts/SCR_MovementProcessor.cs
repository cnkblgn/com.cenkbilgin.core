namespace Game
{
    public interface IMovementProcessor
    {
        public int Priority { get; }
        public void OnBeforeMove(MovementController controller);
        public void OnBeforeLook(MovementController controller);
    }
}
