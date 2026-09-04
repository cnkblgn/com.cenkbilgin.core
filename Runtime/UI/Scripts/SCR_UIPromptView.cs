using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Core.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    internal sealed class UIPromptView : MonoBehaviour
    {
        public bool IsActive => isActive;

        [Header("_")]
        [SerializeField] private TextMeshProUGUI descriptionText = null;
        [SerializeField, Required] private Button acceptButton = null;
        [SerializeField, Required] private Button cancelButton = null;

        private CanvasGroup thisCanvas;
        private IUIPromptHandler thisHandler;
        private bool isActive = true;

        private void Awake()
        {
            thisCanvas = GetComponent<CanvasGroup>();
            thisHandler = GetComponent<IUIPromptHandler>();
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
            thisHandler?.Accept();
            TryHide();
        }
        private void OnCancelButtonClicked()
        {
            TryHide();
        }

        public bool TryShow<TContext>(string description, in TContext context) where TContext : struct
        {
            if (isActive)
            {
                return false;
            }

            if (thisHandler is not IUIPromptHandler<TContext> handler)
            {
                return false;
            }

            isActive = true;
            thisCanvas.Show();
            handler.Show(context);

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            return true;
        }
        public bool TryHide()
        {
            if (!isActive)
            {
                return false;
            }

            isActive = false;
            thisCanvas.Hide();
            thisHandler?.Cancel();
            thisHandler?.Hide();

            return true;
        }
    }
}