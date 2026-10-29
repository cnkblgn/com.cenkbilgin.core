using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    public sealed class UIOptionSlider : UIOption<float>
    {
        [Header("_")]
        [SerializeField, Required] private Slider sliderObject = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private TextMeshProUGUI valueText = null;

        private string[] valueTexts;
        private bool showMax;

        private void Awake() => sliderObject.onValueChanged.AddListener(Set);
        private void OnDisable() => sliderObject.onValueChanged.RemoveListener(Set);

        public UIOptionSlider Initialize(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged, bool showMax = false)
        {
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            this.showMax = showMax;

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

            string text = valueTexts != null && (int)value < valueTexts.Length ? valueTexts[(int)value] : value.ToString("0.00");
            valueText.text = showMax ? $"{text} / {sliderObject.maxValue:0}" : text;
        }
    }
}