using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    using static CoreUtility;

    public sealed class PoolSystem<T> where T : Component
    {
        public string ID { get; }
        public PoolType Type { get; }

        public int TotalCount => currentItems.Length;
        public int ActiveCount => Type != PoolType.RELEASE ? -1 : TotalCount - availableItems.Count;
        public int AvailableCount => Type != PoolType.RELEASE ? -1 : availableItems.Count;

        private readonly IPoolHandler<T> currentHandler;
        private readonly T[] currentItems;
        private readonly Queue<T> availableItems;
        private readonly Transform container;
        private int currentIndex = 0;
        private int currentDirection = 1;

        public PoolSystem(string id, PoolType type, T prefab, Transform container, int count, IPoolHandler<T> handler)
        {
            Type = type;
            ID = id;

            currentItems = new T[Type == PoolType.SINGLE ? 1 : count];
            currentHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            availableItems = new();

            this.container = container;

            for (int i = 0; i < TotalCount; i++)
            {
                T item = GameObject.Instantiate<T>(prefab, container);

                InitializeItem(item);

                currentItems[i] = item;

                if (Type == PoolType.RELEASE)
                {
                    availableItems.Enqueue(item);
                }
            }
        }

        private void InitializeItem(T item)
        {
            item.gameObject.SetActive(false);
            currentHandler.HandleInitialization(item);
        }

        public void Reset(bool reparent, bool deactivate)
        {
            availableItems.Clear();

            foreach (T item in currentItems)
            {
                ResetItem(item, reparent, deactivate);

                if (Type == PoolType.RELEASE)
                {
                    availableItems.Enqueue(item);
                }
            }
        }
        private void ResetItem(T item, bool reparent, bool deactivate)
        {
            currentHandler.HandleReset(item);
            if (deactivate) item.gameObject.SetActive(false);
            if (reparent) item.transform.SetParent(container);
        }

        public void Release(T item)
        {
            if (Type != PoolType.RELEASE)
            {
                Debug.LogWarning($"Pool [{ID}] does not support Release");
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

            ResetItem(item, false, true);
            availableItems.Enqueue(item);
        }
        public bool TryGet(int index, out T item)
        {
            item = null;

            if (index < 0 || index >= TotalCount)
            {
                return false;
            }

            item = currentItems[index];
            return true;
        }

        public T GetNext()
        {
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
        private T GetSingle() => currentItems[0];
        private T GetRing()
        {
            T item = currentItems[currentIndex];

            currentIndex = (currentIndex + 1) % currentItems.Length;

            return item;
        }
        private T GetPingPong()
        {
            if (currentItems.Length <= 1)
            {
                return currentItems[0];
            }

            T item = currentItems[currentIndex];

            if (currentIndex == currentItems.Length - 1)
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