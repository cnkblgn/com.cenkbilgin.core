namespace Core.Misc
{
    public interface IMovementControllerProcessor
    {
        public int Priority { get; }
        public void OnAfterCameraTick();
        public void OnAfterMoveTick();
    }
}
