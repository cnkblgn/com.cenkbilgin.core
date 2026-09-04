using System;

namespace Core.UI
{
    public readonly struct UIConfirmationPromptContext
    {
        public readonly Action OnAccept;
        public readonly Action OnCancel;

        public UIConfirmationPromptContext(Action onAccept, Action onCancel, string description)
        {
            OnAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept), "UI confirmation prompt context invalid! accept action missing!?");
            OnCancel = onCancel;
        }
    }
}