using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Scenario
{
    public sealed class ScenarioContainer
    {
        public event Action<ScenarioContext> OnChanged;

        private ScenarioID id;
        private ScenarioContext ctx;
        private readonly List<IScenarioHandler> handlers;

        public ScenarioContainer(ScenarioContainer container, List<IScenarioHandler> handlers = null) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id, handlers) { }
        public ScenarioContainer(ScenarioID id, List<IScenarioHandler> handlers = null)
        {
            this.id = id;
            this.handlers = handlers == null ? new() : new(handlers);
        }

        private void NotifyHandlers(in ScenarioContext ctx)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                handlers[i]?.HandleScenarioChanged(in ctx);
            }
        }
        public void AddHandler(IScenarioHandler handler)
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
        public void RemoveHandler(IScenarioHandler handler)
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

        public ScenarioContext GetContext() => ctx;
        public ScenarioID GetScenario() => id;
        public bool TrySetScenario<TContext>(ScenarioID id, TContext context)
        {
            if (id == this.id)
            {
                return false;
            }

            if (!id.IsValid)
            {
                Debug.LogError($"Scenario set failed! {id} is not valid!");
                return false;
            }

            RemoveAction(this.id, context);

            ctx = new(this.id = id);
            NotifyHandlers(in ctx);
            OnChanged?.Invoke(ctx);

            ApplyAction(id, context);

            return true;
        }

        private void ApplyAction<TContext>(ScenarioID id, TContext context)
        {
            ScenarioAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is ScenarioAction<TContext> action)
                {
                    action.Apply(id, context);
                }
            }
        }
        private void RemoveAction<TContext>(ScenarioID id, TContext context)
        {
            ScenarioAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] is ScenarioAction<TContext> action)
                {
                    action.Remove(id, context);
                }
            }
        }
    }
}
