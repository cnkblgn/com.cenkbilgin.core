namespace Game
{
    public interface IMovementModule
    {
        public int Priority { get; }
        public void Bind(MovementController controller);
        public void Unbind(MovementController controller);
        public void OnStateChanged(MovementContext ctx);
        public void OnBeforeMove();
        public void OnBeforeLook();
    }
}
