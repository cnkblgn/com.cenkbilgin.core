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

        public void HandleShow(in UIConfirmationPromptContext ctx)
        {
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;
        }
        public void HandleAccept() => onAcceptEvent?.Invoke();
        public void HandleCancel() => onCancelEvent?.Invoke();
        public void HandleHide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
