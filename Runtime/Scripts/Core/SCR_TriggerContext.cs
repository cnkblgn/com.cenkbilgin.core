using System;
using UnityEngine;

namespace Core
{
    public readonly struct TriggerContext
    {
        public readonly TriggerState State;
        public readonly Collider Collider;

        public TriggerContext(TriggerState state, Collider collider)
        {
            State = state;
            Collider = collider != null ? collider : throw new ArgumentNullException(nameof(collider));
        }
    }
}
