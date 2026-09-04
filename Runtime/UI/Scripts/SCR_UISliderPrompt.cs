using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIPromptView))]
    internal sealed class UISliderPrompt : MonoBehaviour, IUIPromptHandler<UISliderPromptContext>
    {
        [Header("_")]
        [SerializeField, Required] private Slider slider;

        private Action<float> onAcceptEvent = null;
        private Action onCancelEvent = null;

        public void Show(in UISliderPromptContext ctx)
        {
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;

            slider.minValue = ctx.Min;
            slider.maxValue = ctx.Max;
            slider.value = slider.value;
        }
        public void Accept() => onAcceptEvent?.Invoke(slider.value);
        public void Cancel() => onCancelEvent?.Invoke();
        public void Hide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
