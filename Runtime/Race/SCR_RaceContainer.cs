using System;
using UnityEngine;

namespace Core.Race
{
    public class RaceContainer
    {
        public event Action<RaceID> OnChanged;

        private RaceID id;

        public RaceContainer(RaceContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id) { }
        public RaceContainer(RaceID id) => this.id = !id.IsValid ? throw new ArgumentNullException(nameof(id)) : id;

        public RaceID GetRace() => id;
        public bool TrySetRace(RaceID id, object context)
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

            if (context == null)
            {
                Debug.LogError("Race set failed! actor is null!?");
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

        private void ApplyAction(RaceID id, object context)
        {
            RaceAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Apply(id, context);
            }
        }
        private void RemoveAction(RaceID id, object context)
        {
            RaceAction[] actions = id.GetDefinition().Actions;

            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Remove(id, context);
            }
        }
    }
}
