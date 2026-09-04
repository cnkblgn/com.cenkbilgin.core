using System;
using UnityEngine;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIPromptView))]
    internal sealed class UIConfirmationPrompt : MonoBehaviour, IUIPromptHandler<UIConfirmationPromptContext>
    {
        private Action onAcceptEvent = null;
        private Action onCancelEvent = null;

        public void Show(in UIConfirmationPromptContext ctx)
        {
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;
        }
        public void Accept() => onAcceptEvent?.Invoke();
        public void Cancel() => onCancelEvent?.Invoke();
        public void Hide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
