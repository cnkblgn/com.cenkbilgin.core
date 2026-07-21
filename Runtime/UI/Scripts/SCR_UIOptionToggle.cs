using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UIOptionToggle : UIOption<bool>
    {
        [Header("_")]
        [SerializeField, Required] private Toggle toggleObject = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;

        private void Awake() => toggleObject.onValueChanged.AddListener(Set);
        private void OnDisable() => toggleObject.onValueChanged.RemoveListener(Set);

        public UIOptionToggle Initialize(bool initial, bool @default, string description, Action<bool> onApply, Action<bool> onChanged)
        {
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            base.Initialize(initial, @default, onApply, onChanged);
            return this;
        }
        protected override void SetInternal(bool value) => toggleObject.SetIsOnWithoutNotify(value);
    }
}