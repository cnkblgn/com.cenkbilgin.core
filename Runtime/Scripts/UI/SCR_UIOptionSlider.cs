using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    public class UIOptionSlider : UIOption<float>
    {
        [Header("_")]
        [SerializeField, Required] private Slider sliderObject = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private TextMeshProUGUI valueText = null;

        private string[] valueTexts = null;

        private void OnDisable() => sliderObject.onValueChanged.RemoveAllListeners();
        public UIOptionSlider Initialize(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged)
        {
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            this.sliderObject.minValue = minValue;
            this.sliderObject.maxValue = maxValue;
            this.sliderObject.wholeNumbers = isInt;
            this.sliderObject.onValueChanged.AddListener(Set);

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

            if (valueText != null)
            {
                if (valueTexts != null)
                {
                    valueText.text = valueTexts[(int)value];
                }
                else
                {
                    valueText.text = value.ToString(STRING_FORMAT_000);
                }              
            }

            if (isInitialized)
            {
                onChanged?.Invoke(value);
            }
        }
    }
}
