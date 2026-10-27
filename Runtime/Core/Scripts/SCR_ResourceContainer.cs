using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [Serializable]
    public sealed class ResourceContainer
    {
        public event Action<ResourceContext> OnChanged;

        [Header("_")]
        [SerializeField, Min(0)] private float current;
        [SerializeField, Min(0)] private float maximum;

        private readonly List<IResourceHandler> handlers;
        private bool isDepleted;

        public ResourceContainer() : this(0, 0, null) { }
        public ResourceContainer(ResourceContainer container, List<IResourceHandler> handlers = null) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.maximum, container.current, handlers) { }
        public ResourceContainer(float current, float maximum, List<IResourceHandler> handlers = null)
        {
            this.current = current;
            this.maximum = maximum;
            this.handlers = handlers == null ? new() : new(handlers);

            isDepleted = false;
        }

        private void NotifyHandlers(in ResourceContext ctx)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleResourceChanged(in ctx);
            }
        }
        public void AddHandler(IResourceHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (handlers.Contains(handler))
            {
                return;
            }

            handlers.Add(handler);
        }
        public void RemoveHandler(IResourceHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!handlers.Contains(handler))
            {
                return;
            }

            handlers.Remove(handler);
        }
        private void SetState(ResourceState state)
        {
            ResourceContext ctx = new(state, current, maximum);
            NotifyHandlers(in ctx);
            OnChanged?.Invoke(ctx);
        }

        public bool IsDepleted() => isDepleted;

        public float Recover(float amount)
        {
            if (amount <= 0f)
            {
                return amount;
            }

            float before = current;

            SetCurrent(current + amount);

            float added = current - before;

            return amount - added;
        }
        public float Consume(float amount)
        {
            if (amount <= 0f)
            {
                return amount;
            }

            float before = current;

            SetCurrent(current - amount);

            float consumed = before - current;

            return amount - consumed;
        }

        public float GetCurrent() => current;
        public void SetCurrent(float value)
        {
            float old = current;

            current = Mathf.Clamp(value, 0, maximum);

            if (!Mathf.Approximately(old, current))
            {
                SetState(ResourceState.CHANGED);
            }

            if (current <= 0 && !isDepleted)
            {
                isDepleted = true;
                SetState(ResourceState.DEPLETED);
            }
            else if (current > 0 && isDepleted)
            {
                isDepleted = false;
            }
        }

        public float GetMaximum() => maximum;
        public void SetMaximum(float value)
        {
            maximum = Mathf.Max(0f, value);

            SetCurrent(current);
        }
    }
}
