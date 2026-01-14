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

        [Header("_")]
        [SerializeField] private UIPointerEventTooltip thisTooltip = null;

        private string[] valueTexts = null;

        private void OnDisable() => sliderObject.onValueChanged.RemoveAllListeners();
        public UIOptionSlider Initialize(float initial, float @default, string description, float minValue, float maxValue, bool isInt, Action<float> onApply, Action<float> onChanged)
        {
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            if (thisTooltip != null)
            {
                thisTooltip.Initialize(this.sliderObject.value.ToString(STRING_FORMAT_000), true);
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
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            if (thisTooltip != null)
            {
                thisTooltip.Initialize(this.sliderObject.value.ToString(STRING_FORMAT_000), true);
            }

            this.valueTexts = values;
            this.sliderObject.minValue = 0;
            this.sliderObject.maxValue = values.Length - 1;
            this.sliderObject.wholeNumbers = true;
            this.sliderObject.onValueChanged.AddListener(Set);

            base.Initialize(initial, @default, onApply, onChanged);

            return this;
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

            if (thisTooltip != null)
            {
                if (valueTexts != null)
                {
                    thisTooltip.Initialize(valueTexts[(int)value], true);
                }
                else
                {
                    thisTooltip.Initialize(value.ToString(STRING_FORMAT_000), true);
                }

                if (thisTooltip.IsFocused)
                {
                    thisTooltip.Show();
                }
            }

            if (isInitialized)
            {
                onChanged?.Invoke(value);
            }
        }
    }
}
