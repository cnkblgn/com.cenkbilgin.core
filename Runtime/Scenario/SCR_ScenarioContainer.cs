using System;
using UnityEngine;

namespace Core.Scenario
{
    public class ScenarioContainer
    {
        public event Action<ScenarioContext> OnChanged;

        private readonly IScenarioHandler user;
        private ScenarioID id;

        public ScenarioContainer(IScenarioHandler user = null) : this(default, user) { }
        public ScenarioContainer(ScenarioContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id, container.user) { }
        public ScenarioContainer(ScenarioID id, IScenarioHandler user = null)
        {
            this.id = id;
            this.user = user;
        }

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

            ScenarioContext ctx = new(this.id = id);
            user?.HandleScenarioChanged(ctx);
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
