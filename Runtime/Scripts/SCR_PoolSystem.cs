using System;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    [Serializable]
    public abstract class PoolSystem<T> where T : Component
    {
        public PoolType Type { get; private set; } = PoolType.RING_BUFFER;
        public string ID { get; private set; } = STRING_EMPTY;

        protected Transform thisContainer = null;
        private T[] thisItems = new T[0] { };
        private int currentIndex = 0;
        private bool isInitialized = false;

        public void Initialize(T prefab, Transform container, PoolType type, string id, int count)
        {
            if (isInitialized)
            {
                return;
            }

            this.ID = id;
            this.Type = type;
            this.thisContainer = container;
            this.thisItems = new T[count];

            count = type == PoolType.SINGLE ? 1 : count;

            for (int i = 0; i < count; i++)
            {
                T item = GameObject.Instantiate<T>(prefab, container);
                item.gameObject.SetActive(false);

                OnInitialize(item);
                thisItems[i] = item;
            }

            isInitialized = true;
        }
        protected abstract void OnInitialize(T item);
        public void Reset()
        {
            if (!isInitialized)
            {
                return;
            }

            foreach (T item in thisItems)
            {
                OnReset(item);
            }
        }
        protected abstract void OnReset(T item);

        protected T GetNext()
        {
            if (!isInitialized)
            {
                Debug.LogError($"PoolSystem.GetNext() pool system is not initialized!");
                return null;
            }

            switch (Type)
            {
                case PoolType.SINGLE:
                    return GetSingle();
                case PoolType.RING_BUFFER:
                    return GetRing();
                default:
                    Debug.LogWarning($"PoolSystem.GetNext() [{Type}] not defined");
                    break;
            }

            return default;
        }
        private T GetSingle() => thisItems[0];
        private T GetRing()
        {
            T item = thisItems[currentIndex];

            currentIndex = (currentIndex + 1) % thisItems.Length;

            return item;
        }
    }
}