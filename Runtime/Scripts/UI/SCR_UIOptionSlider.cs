using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UIOptionSlider : UIOption<float>
    {
        [Header("_")]
        [SerializeField, Required] private Slider sliderObject = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private TextMeshProUGUI valueText = null;

        private string[] valueTexts = default;

        private void Awake() => sliderObject.onValueChanged.AddListener(Set);
        private void OnDisable() => sliderObject.onValueChanged.RemoveListener(Set);

        public UIOptionSlider Initialize(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged)
        {
            valueTexts = null;

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            sliderObject.minValue = minValue;
            sliderObject.maxValue = maxValue;
            sliderObject.wholeNumbers = isInt;

            base.Initialize(initial, @default, onApply, onChanged);
            return this;
        }
        public UIOptionSlider Initialize(float initial, float @default, string description, string[] values, Action<float> onApply, Action<float> onChanged)
        {
            valueTexts = values;
            return Initialize(initial, @default, description, 0, values.Length - 1, true, onApply, onChanged);
        }

        protected override void SetInternal(float value)
        {
            sliderObject.SetValueWithoutNotify(value);

            if (valueText == null)
            {
                return;
            }

            if (valueTexts != null && (int)value < valueTexts.Length)
            {
                valueText.text = valueTexts[(int)value];
            }
            else
            {
                valueText.text = value.ToString(STRING_FORMAT_000);
            }
        }
    }
}