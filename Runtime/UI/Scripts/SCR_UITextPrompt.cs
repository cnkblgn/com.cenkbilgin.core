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

        public void Show(in UITextPromptContext ctx)
        {
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;
        }
        public void Accept() => onAcceptEvent?.Invoke(input.text);
        public void Cancel() => onCancelEvent?.Invoke();
        public void Hide()
        {
            onAcceptEvent = null;
            onCancelEvent = null;
        }
    }
}
