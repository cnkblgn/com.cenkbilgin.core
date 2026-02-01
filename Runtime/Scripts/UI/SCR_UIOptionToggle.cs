using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    public class UIOptionToggle : UIOption<bool>
    {
        [Header("_")]
        [SerializeField, Required] private Toggle toggleObject = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;

        private void OnDisable() => toggleObject.onValueChanged.RemoveAllListeners();
        public UIOptionToggle Initialize(bool initial, bool @default, string description, Action<bool> onApply, Action<bool> onChanged)
        {
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            this.toggleObject.onValueChanged.AddListener(Set);

            base.Initialize(initial, @default, onApply, onChanged);

            return this;
        }
        protected override void SetInternal(bool value)
        {
            toggleObject.SetIsOnWithoutNotify(value);

            if (isInitialized)
            {
                onChanged?.Invoke(value);
            }
        }
    }
}