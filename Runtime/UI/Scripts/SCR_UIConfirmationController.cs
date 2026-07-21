using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    internal sealed class UIConfirmationController : MonoBehaviour
    {
        [Header("_")]
        [SerializeField] private TextMeshProUGUI confirmationText = null;

        [Header("_")]
        [SerializeField, Required] private Button acceptButton = null;
        [SerializeField, Required] private Button cancelButton = null;

        private Canvas thisCanvas = null;
        private Action onAcceptEvent = null;
        private Action onCancelEvent = null;
        private bool isOpened = false;
        private bool hideCursor = true;

        private void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
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
            onAcceptEvent?.Invoke();

            Hide();
        }
        private void OnCancelButtonClicked()
        {
            onCancelEvent?.Invoke();

            Hide();
        }

        public void Show(in UIConfirmationContext ctx)
        {
            if (isOpened)
            {
                return;
            }

            ManagerUI.Instance.ShowCursor();

            thisCanvas.Show();
            onAcceptEvent = ctx.OnAccept;
            onCancelEvent = ctx.OnCancel;
            confirmationText.text = ctx.Text;
            hideCursor = ctx.HideCursor;
            isOpened = true;
        }
        public void Hide()
        {
            if (hideCursor)
            {
                ManagerUI.Instance.HideCursor();
            }

            thisCanvas.Hide();
            onAcceptEvent = null;
            onCancelEvent = null;
            isOpened = false;
        }
    }
}