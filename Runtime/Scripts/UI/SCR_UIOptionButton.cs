using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public class UIOptionButton : UIOption<int>
    {
        [Header("_")]
        [SerializeField, Required] private Button nextButton = null;
        [SerializeField, Required] private Button previousButton = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private TextMeshProUGUI valueText = null;

        [Header("_")]
        [SerializeField] private UIPointerEventTooltip thisTooltip = null;

        private string[] valueTexts = null;
        private int maximumIndex = 0;
        private int currentIndex = 0;
        private bool wrapAround = false;

        private void OnDisable()
        {
            nextButton.onClick.RemoveAllListeners();
            previousButton.onClick.RemoveAllListeners();
        }
        public UIOptionButton Initialize(int initial, int @default, string description, string[] values, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            this.currentIndex = 0;
            this.valueTexts = values;
            this.maximumIndex = values.Length - 1;
            this.wrapAround = wrapAround;

            if (thisTooltip != null)
            {
                thisTooltip.Initialize(values[currentIndex], true);
            }

            nextButton.onClick.AddListener(SetForward);
            previousButton.onClick.AddListener(SetPrevious);

            base.Initialize(initial, @default, onApply, onChanged);

            return this;
        }
        public UIOptionButton Initialize(int initial, int @default, string description, int maximumIndex, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            if (this.descriptionText != null)
            {
                this.descriptionText.text = description;
            }

            this.currentIndex = 0;
            this.maximumIndex = maximumIndex;
            this.wrapAround = wrapAround;

            if (thisTooltip != null)
            {
                thisTooltip.Initialize(currentIndex.ToString(), true);
            }

            nextButton.onClick.AddListener(SetForward);
            previousButton.onClick.AddListener(SetPrevious);

            base.Initialize(initial, @default, onApply, onChanged);

            return this;
        }
        private void SetForward() => Set(currentIndex + 1);
        private void SetPrevious() => Set(currentIndex - 1);
        protected override void SetInternal(int value)
        {
            if (wrapAround)
            {
                value = value > maximumIndex ? 0 : value < 0 ? maximumIndex : value;
            }

            currentIndex = Mathf.Clamp(value, 0, maximumIndex);

            if (valueText != null)
            {
                if (valueTexts != null && currentIndex < valueTexts.Length)
                {
                    valueText.text = valueTexts[currentIndex];
                }
                else
                {
                    valueText.text = currentIndex.ToString();
                }
            }

            if (thisTooltip != null)
            {
                if (valueTexts != null)
                {
                    thisTooltip.Initialize(valueTexts[currentIndex], true);
                }
                else
                {
                    thisTooltip.Initialize(currentIndex.ToString(), true);
                }

                if (thisTooltip.IsFocused)
                {
                    thisTooltip.Show(null);
                }
            }

            if (isInitialized)
            {
                onChanged?.Invoke(value);
            }
        }
    }
}
