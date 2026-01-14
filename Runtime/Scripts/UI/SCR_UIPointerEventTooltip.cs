using Core;
using Core.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public class UIPointerEventTooltip : UIPointerEvent
    {
        public bool IsFocused { get; private set; } = false;

        [Header("_")]
        [SerializeField, TextArea()] private string baseText = STRING_EMPTY;
        [SerializeField] private string localizedID = STRING_EMPTY;

        private bool autoUpdate = true;
        private bool isOpened = false;
        private string thisText = STRING_EMPTY;
        private string localizedText = STRING_EMPTY;

        private void Start()
        {
            if (!string.IsNullOrEmpty(localizedID))
            {
                localizedText = ManagerCoreLocalization.Instance.Get(localizedID);
            }
        }
        private void OnDestroy()
        {
            if (!isOpened)
            {
                return;
            }

            Hide();
        }

        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            IsFocused = true;

            if (!autoUpdate)
            {
                return;
            }

            Show();
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            base.OnPointerExitInternal(eventData);

            IsFocused = false;

            if (!autoUpdate)
            {
                return;
            }

            Hide();
        }

        public void Initialize(string text, bool autoUpdate)
        {
            this.autoUpdate = autoUpdate;
            thisText = text;
        }
        public virtual void Show()
        {
            ManagerCoreUI.Instance.ShowTooltip(baseText + localizedText + thisText);
            isOpened = true;
        }
        public virtual void Hide()
        {
            ManagerCoreUI.Instance.HideTooltip();
            isOpened = false;
        }
    }
}