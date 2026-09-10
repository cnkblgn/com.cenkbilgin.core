using System;
using UnityEngine;
using TMPro;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIPromptView))]
    internal sealed class UITextPrompt : MonoBehaviour, IUIPromptHandler<UITextPromptContext>
    {
        [Header("_")]
        [SerializeField, Required] private TMP_InputField input;

        private Action<string> onAcceptEvent = null;
        private Action onCancelEvent = null;

        public void HandleShow(in UITextPromptContext ctx)
        {
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;
        }
        public void HandleAccept() => onAcceptEvent?.Invoke(input.text);
        public void HandleCancel() => onCancelEvent?.Invoke();
        public void HandleHide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
