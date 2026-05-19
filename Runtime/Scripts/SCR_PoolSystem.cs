using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public abstract class PoolSystem<T> where T : Component
    {
        public abstract PoolType Type { get; }
        public abstract string ID { get; }

        public int TotalCount => thisItems.Length;
        public int ActiveCount => Type != PoolType.RELEASE ? -1 : TotalCount - availableItems.Count;
        public int AvailableCount => Type != PoolType.RELEASE ? -1 : availableItems.Count;

        protected Transform thisContainer = null;
        private T[] thisItems = new T[0] { };
        private readonly Queue<T> availableItems = new();
        private int currentIndex = 0;
        private int currentDirection = 1;
        private bool isInitialized = false;

        public void Initialize(T prefab, Transform container, int count)
        {
            if (isInitialized)
            {
                return;
            }

            count = Type == PoolType.SINGLE ? 1 : count;

            thisContainer = container;
            thisItems = new T[count];

            for (int i = 0; i < count; i++)
            {
                T item = GameObject.Instantiate<T>(prefab, container);

                item.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                item.gameObject.SetActive(false);

                OnInitialize(item);

                thisItems[i] = item;

                if (Type == PoolType.RELEASE) availableItems.Enqueue(item);
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

            availableItems.Clear();

            foreach (T item in thisItems)
            {
                OnReset(item);

                if (Type == PoolType.RELEASE)
                {
                    availableItems.Enqueue(item);
                }
            }
        }
        protected abstract void OnReset(T item);

        public void Release(T item)
        {
            if (!isInitialized)
            {
                Debug.LogError($"Pool [{ID}] not initialized");
                return;
            }

            if (Type != PoolType.RELEASE)
            {
                Debug.LogWarning($"Pool [{ID}] does not support Release()");
                return;
            }

            if (item == null)
            {
                Debug.LogError($"Pool [{ID}] Release item == null");
                return;
            }

#if UNITY_EDITOR
            if (availableItems.Contains(item))
            {
                Debug.LogWarning($"Pool [{ID}] double release detected on [{item.name}]");
                return;
            }
#endif

            OnReset(item);

            item.gameObject.SetActive(false);

            availableItems.Enqueue(item);
        }
        public T Get(int index) => thisItems[index];

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
                case PoolType.PING_PONG:
                    return GetPingPong();
                case PoolType.RELEASE:
                    return GetRelease();
                default:
                    Debug.LogWarning($"[{Type}] not defined");
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
        private T GetPingPong()
        {
            if (thisItems.Length <= 1)
            {
                return thisItems[0];
            }

            T item = thisItems[currentIndex];

            if (currentIndex == thisItems.Length - 1)
            {
                currentDirection = -1;
            }
            else if (currentIndex == 0)
            {
                currentDirection = 1;
            }

            currentIndex += currentDirection;

            return item;
        }
        private T GetRelease()
        {
            if (availableItems.Count == 0)
            {
                Debug.LogWarning($"Pool [{ID}] exhausted!");
                return null;
            }

            T item = availableItems.Dequeue();

            if (item == null)
            {
                Debug.LogError($"Pool [{ID}] item destroyed illegally!");
                return null;
            }

            return item;
        }
    }
}