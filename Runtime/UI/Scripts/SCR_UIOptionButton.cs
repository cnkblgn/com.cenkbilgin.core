using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UIOptionButton : UIOption<int>
    {
        [Header("_")]
        [SerializeField, Required] private Button nextButton = null;
        [SerializeField, Required] private Button previousButton = null;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField] private TextMeshProUGUI valueText = null;

        private string[] valueTexts = null;
        private int maximumIndex = 0;
        private bool wrapAround = false;

        private void Awake()
        {
            nextButton.onClick.AddListener(SetForward);
            previousButton.onClick.AddListener(SetPrevious);
        }
        private void OnDisable()
        {
            nextButton.onClick.RemoveListener(SetForward);
            previousButton.onClick.RemoveListener(SetPrevious);
        }

        public UIOptionButton Initialize(int initial, int @default, string description, int maximumIndex, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            this.maximumIndex = maximumIndex;
            this.wrapAround = wrapAround;

            base.Initialize(initial, @default, onApply, onChanged);
            return this;
        }
        public UIOptionButton Initialize(int initial, int @default, string description, string[] values, Action<int> onApply, Action<int> onChanged, bool wrapAround = false)
        {
            valueTexts = values;

            return Initialize(initial, @default, description, values.Length - 1, onApply, onChanged, wrapAround);
        }

        private void SetForward() => Set(currentValue + 1);
        private void SetPrevious() => Set(currentValue - 1);

        protected override int Validate(int value)
        {
            if (wrapAround)
            {
                if (value > maximumIndex)
                {
                    return 0;
                }

                if (value < 0)
                {
                    return maximumIndex;
                }
            }

            return Mathf.Clamp(value, 0, maximumIndex);
        }
        protected override void SetInternal(int value)
        {
            if (valueText == null)
            {
                return;
            }

            if (valueTexts != null && value < valueTexts.Length)
            {
                valueText.text = valueTexts[value];
            }
            else
            {
                valueText.text = value.ToString();
            }
        }
    }
}