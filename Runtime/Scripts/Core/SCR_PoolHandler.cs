namespace Core
{
    public interface IPoolHandler<T>
    {
        /// <summary> Called when pool item is created. </summary>
        public void HandleInitialization(T item);
        /// <summary> Called when pool item returned to pool. eg: OnBeforeSceneChange or Pool.Release() </summary>
        public void HandleReset(T item);
    }
}
