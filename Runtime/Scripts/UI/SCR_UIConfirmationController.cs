using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIConfirmationController : MonoBehaviour
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
        public void Show(string confirmationText, Action onAcceptEvent, Action onCancelEvent)
        {
            if (onAcceptEvent == null)
            {
                LogError("UIConfirmationController.Show() onAcceptEvent == null!");
                return;
            }

            if (isOpened)
            {
                return;
            }

            ManagerCoreUI.Instance.ShowCursor();

            thisCanvas.Show();
            this.onAcceptEvent = onAcceptEvent;
            this.onCancelEvent = onCancelEvent;
            this.confirmationText.text = confirmationText;
            isOpened = true;
        }
        public void Hide()
        {
            if (!ManagerCoreGame.Instance.IsStartingScene() && !ManagerCoreGame.Instance.IsBootstrapScene() && ManagerCoreGame.Instance.GetGameState() != GameState.PAUSE)
            {
                ManagerCoreUI.Instance.HideCursor();
            }

            thisCanvas.Hide();
            onAcceptEvent = null;
            onCancelEvent = null;
            isOpened = false;
        }
    }
}