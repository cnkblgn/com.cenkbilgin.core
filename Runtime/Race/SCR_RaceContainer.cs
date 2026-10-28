using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Race
{
    public sealed class RaceContainer
    {
        public event Action<RaceContext> OnChanged;

        private RaceID id;
        private RaceContext ctx;
        private readonly List<IRaceHandler> handlers;

        public RaceContainer(RaceContainer container, List<IRaceHandler> handlers = null) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id, handlers) { }
        public RaceContainer(RaceID id, List<IRaceHandler> handlers = null)
        {
            this.id = id;
            this.handlers = handlers == null ? new() : new(handlers);
        }

        private void NotifyHandlers(in RaceContext ctx)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleRaceChanged(in ctx);
            }
        }
        public void AddHandler(IRaceHandler handler)
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
        public void RemoveHandler(IRaceHandler handler)
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

        public RaceContext GetContext() => ctx;
        public RaceID GetRace() => id;
        public bool TrySetRace<TContext>(RaceID id, TContext context)
        {
            if (id == this.id)
            {
                return false;
            }

            if (!id.IsValid)
            {
                Debug.LogError($"Race set failed! {id} is not valid!");
                return false;
            }

            RemoveAction(this.id, context);

            ctx = new(this.id = id);
            NotifyHandlers(in ctx);
            OnChanged?.Invoke(ctx);

            ApplyAction(id, context);

            return true;
        }

        private void ApplyAction<TContext>(RaceID id, TContext context)
        {
            RaceAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is RaceAction<TContext> action)
                {
                    action.Apply(id, context);
                }
            }
        }
        private void RemoveAction<TContext>(RaceID id, TContext context)
        {
            RaceAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is RaceAction<TContext> action)
                {
                    action.Remove(id, context);
                }
            }
        }
    }
}
