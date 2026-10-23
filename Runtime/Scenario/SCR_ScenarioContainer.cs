using System;
using UnityEngine;

namespace Core.Scenario
{
    public class ScenarioContainer
    {
        public event Action<ScenarioID> OnChanged;

        private ScenarioID id;

        public ScenarioContainer(ScenarioContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id) { }
        public ScenarioContainer(ScenarioID id) => this.id = !id.IsValid ? throw new ArgumentNullException(nameof(id)) : id;

        public ScenarioID GetScenario() => id;
        public bool TrySetScenario(ScenarioID id, object context)
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

            if (context == null)
            {
                Debug.LogError("Scenario set failed! actor is null!?");
                return false;
            }

            if (!id.IsCompatibleWith(context))
            {
                return false;
            }

            RemoveAction(id, context);

            OnChanged?.Invoke(this.id = id);

            ApplyAction(id, context);

            return true;
        }

        private void ApplyAction(ScenarioID id, object context)
        {
            ScenarioAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(id, context);
            }
        }
        private void RemoveAction(ScenarioID id, object context)
        {
            ScenarioAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Remove(id, context);
            }
        }
    }
}
