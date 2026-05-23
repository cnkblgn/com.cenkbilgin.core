using UnityEngine;

namespace Core
{
    public interface IPoolHandler<T>
    {
        public void OnInitialize(T item);
        public void OnReset(T item);
    }
}
