using System;

namespace Core.UI
{
    public readonly struct UIConfirmationContext
    {
        public readonly string Text;
        public readonly Action OnAccept;
        public readonly Action OnCancel;
        public readonly bool HideCursor;

        public UIConfirmationContext(Action onAccept, Action onCancel, string text, bool hideCursor)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            OnAccept = onAccept ?? throw new ArgumentNullException(nameof(onAccept));
            OnCancel = onCancel;
            HideCursor = hideCursor;
        }
    }
}