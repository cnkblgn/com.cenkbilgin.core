using System;
using UnityEngine;

namespace Core.Scenario
{
    public class ScenarioContainer
    {
        public event Action<ScenarioContext> OnChanged;

        private readonly IScenarioUser user;
        private ScenarioID id;

        public ScenarioContainer(IScenarioUser user = null) : this(default, user) { }
        public ScenarioContainer(ScenarioContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id, container.user) { }
        public ScenarioContainer(ScenarioID id, IScenarioUser user = null)
        {
            this.id = id;
            this.user = user;
        }

        public ScenarioID GetScenario() => id;
        public bool TrySetScenario(ScenarioID id)
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

            ScenarioContext ctx = new(this.id = id);
            user?.HandleScenarioStateChanged(ctx);
            OnChanged?.Invoke(ctx);
            return true;
        }
    }
}
