using System;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    public abstract class UIOptionBase : MonoBehaviour
    {
        public abstract void Load();
        public abstract void Apply();
        public abstract void Revert();
    }

    public abstract class UIOption<T> : UIOptionBase
    {
        protected Action<T> onApply;
        protected Action<T> onChanged;
        protected T currentValue;
        protected T appliedValue;
        protected T defaultValue;
        protected bool isInitialized;

        public void Initialize(T initial, T @default, Action<T> onApply, Action<T> onChanged)
        {
            this.defaultValue = @default;
            this.onApply = onApply;
            this.onChanged = onChanged;

            isInitialized = false;
            appliedValue = initial;

            Set(initial, false);

            isInitialized = true;
        }

        public override void Apply()
        {
            appliedValue = currentValue;
            onApply?.Invoke(appliedValue);
        }
        public override void Load()
        {
            Set(appliedValue, false);
        }
        public override void Revert()
        {
            appliedValue = defaultValue;
            Load();
        }

        protected virtual T Validate(T value) => value;

        public void Set(T value) => Set(value, true);
        private void Set(T value, bool notify)
        {
            currentValue = Validate(value);

            SetInternal(currentValue);

            if (notify && isInitialized)
            {
                onChanged?.Invoke(currentValue);
            }
        }
        protected abstract void SetInternal(T value);
    }
}