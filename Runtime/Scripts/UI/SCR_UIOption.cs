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
        protected Action<T> onApply = null;
        protected Action<T> onChanged = null;
        protected T currentValue = default;
        protected T appliedValue = default;
        protected T defaultValue = default;
        protected bool isInitialized = false;

        protected virtual void OnDestroy()
        {
            onApply = null;
            onChanged = null;
        }

        public void Initialize(T initial, T @default, Action<T> onApply, Action<T> onChanged)
        {
            this.defaultValue = @default;
            this.onApply = onApply;
            this.onChanged = onChanged;

            appliedValue = initial;

            Set(initial);

            isInitialized = true;
        }

        public override void Apply()
        {
            appliedValue = currentValue;
            onApply?.Invoke(appliedValue);
        }
        public override void Load()
        {
            Set(appliedValue);
        }
        public override void Revert()
        {
            appliedValue = defaultValue;

            Load();
        }

        public void Set(T value)
        {
            currentValue = value;
            SetInternal(currentValue);
        }
        protected abstract void SetInternal(T value);
    }
}
