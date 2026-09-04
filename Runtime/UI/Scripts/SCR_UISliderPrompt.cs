using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIPromptView))]
    internal sealed class UISliderPrompt : MonoBehaviour, IUIPromptHandler<UISliderPromptContext>
    {
        [Header("_")]
        [SerializeField, Required] private Slider valueSlider;
        [SerializeField, Required] private TextMeshProUGUI valueText;

        private Action<float> onAcceptEvent = null;
        private Action onCancelEvent = null;
        private UISliderPromptContext context;

        private void OnEnable() => valueSlider.onValueChanged.AddListener(OnValueChanged);
        private void OnDisable() => valueSlider.onValueChanged.AddListener(OnValueChanged);

        private void OnValueChanged(float value) => valueText.text = context.IsInt ? $"{value:0}" : $"{value:0.00}";

        public void Show(in UISliderPromptContext ctx)
        {
            context = ctx;
            onAcceptEvent = context.OnAccept;
            onCancelEvent = context.OnCancel;

            valueSlider.wholeNumbers = context.IsInt;
            valueSlider.minValue = context.Min;
            valueSlider.maxValue = context.Max;
            valueSlider.value = valueSlider.value;
        }
        public void Accept() => onAcceptEvent?.Invoke(valueSlider.value);
        public void Cancel() => onCancelEvent?.Invoke();
        public void Hide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
