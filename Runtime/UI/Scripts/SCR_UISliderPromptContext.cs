using System;

namespace Core.UI
{
    public readonly struct UISliderPromptContext
    {
        public readonly float Current;
        public readonly float Min;
        public readonly float Max;
        public readonly Action<float> OnAccept;
        public readonly Action OnCancel;

        public UISliderPromptContext(float current, float min, float max, Action<float> onAccept, Action onCancel)
        {
            Current = current;
            Min = min;
            Max = max;
            OnAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept), "UI slider prompt context invalid! accept action missing!?");
            OnCancel = onCancel;
        }
    }
}