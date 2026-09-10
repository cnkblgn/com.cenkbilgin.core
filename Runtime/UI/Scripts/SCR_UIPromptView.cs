using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class UIPromptView : MonoBehaviour
    {
        public bool IsActive => thisHandle != default;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField, Required] private Button acceptButton = null;
        [SerializeField, Required] private Button cancelButton = null;

        private CanvasGroup thisCanvas = null;
        private IUIPromptHandler thisHandler = null;
        private IUIPromptUser thisUser = null;
        private UIPromptHandle thisHandle = default;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();
            thisHandler = GetComponent<IUIPromptHandler>();
            thisCanvas.Hide();
        }
        private void OnEnable()
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
        private void OnDisable()
        {
            acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
            cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }

        private void OnAcceptButtonClicked()
        {
            thisHandler?.HandleAccept();

            Hide();
        }
        private void OnCancelButtonClicked()
        {
            Hide();
        }

        public UIPromptHandle Show<TContext>(string description, in TContext context, IUIPromptUser user) where TContext : struct
        {
            if (IsActive)
            {
                return default;
            }

            if (thisHandler is not IUIPromptHandler<TContext> handler)
            {
                return default;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            thisCanvas.Show();
            thisUser = user;
            thisHandle = new(Guid.NewGuid(), this);
            handler.HandleShow(context);
            return thisHandle;
        }
        public bool TryHide(UIPromptHandle handle)
        {
            if (thisHandle != handle)
            {
                return false;
            }

            Hide();
            return true;
        }
        public void Hide()
        {
            if (!IsActive)
            {
                return;
            }

            thisHandle = default;
            thisCanvas.Hide();
            thisHandler?.HandleCancel();
            thisHandler?.HandleHide();
            thisUser?.HandlePromptHide();
            thisUser = null;
        }
    }
}