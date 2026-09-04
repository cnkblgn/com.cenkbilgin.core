using System;

namespace Core.UI
{
    public readonly struct UITextPromptContext
    {
        public readonly Action<string> OnAccept;
        public readonly Action OnCancel;

        public UITextPromptContext(Action<string> onAccept, Action onCancel, string description)
        {
            OnAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept), "UI text prompt context invalid! accept action missing!?");
            OnCancel = onCancel;
        }
    }
}