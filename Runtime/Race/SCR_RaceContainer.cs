using System;
using UnityEngine;

namespace Core.Race
{
    public class RaceContainer
    {
        public event Action<RaceContext> OnChanged;

        private readonly IRaceUser user;
        private RaceID id;

        public RaceContainer(IRaceUser user = null) : this(default, user) { }
        public RaceContainer(RaceContainer container) : this(container == null ? throw new ArgumentNullException(nameof(container)) : container.id) { }
        public RaceContainer(RaceID id, IRaceUser user = null)
        {
            this.id = id;
            this.user = user;
        }

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

            RaceContext ctx = new(this.id = id);
            user?.HandleRaceChanged(ctx);
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
