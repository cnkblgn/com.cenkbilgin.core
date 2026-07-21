using System;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class Resource
    {
        public event Action<ResourceContext> OnChanged;

        [Header("_")]
        [SerializeField, Min(0)] private float current;
        [SerializeField, Min(0)] private float maximum;

        private bool isDepleted;

        public Resource() : this(0, 0) { }
        public Resource(float current, float maximum)
        {
            this.current = current;
            this.maximum = maximum;

            isDepleted = false;
        }

        public float Add(float amount)
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

        private void SetState(ResourceState state) => OnChanged?.Invoke(new(state, current, maximum));

        public bool IsDepleted() => isDepleted;

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
