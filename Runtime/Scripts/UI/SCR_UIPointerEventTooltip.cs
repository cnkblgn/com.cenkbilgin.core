using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public class UIPointerEventTooltip : UIPointerEvent
    {
        public bool IsFocused { get; private set; } = false;

        [Header("_")]
        [SerializeField] private bool disableEventPosition = true;
        [SerializeField, Range(0, 4)] private int cornerIndex = 0;

        [Header("_")]
        [SerializeField, TextArea()] private string baseText = STRING_EMPTY;

        private RectTransform thisTransform = null;
        private readonly Vector3[] cornerOrigins = new Vector3[4];
        private readonly Vector2 cornerOffset = new(16, 16);
        private string runtimeText = STRING_EMPTY;
        private bool autoUpdate = true;
        private bool isOpened = false;

        private void Awake() => thisTransform = GetComponent<RectTransform>();
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
            IsFocused = true;

            if (!autoUpdate)
            {
                return;
            }

            Show(disableEventPosition ? null : eventData.position);
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            IsFocused = false;

            if (!autoUpdate)
            {
                return;
            }

            Hide();
        }
        protected override void OnPointerMoveInternal(PointerEventData eventData)
        {
            if (!IsFocused)
            {
                return;
            }

            if (disableEventPosition)
            {
                return;
            }

            ManagerCoreUI.Instance.MoveTooltip(eventData.position);
        }
        private Vector3 GetPosition()
        {
            Vector3 position;

            if (cornerIndex > 3)
            {
                position = thisTransform.TransformPoint(thisTransform.rect.center);
            }
            else
            {
                thisTransform.GetWorldCorners(cornerOrigins);
                float x = (cornerIndex == 0 || cornerIndex == 1) ? +cornerOffset.x : -cornerOffset.x;
                float y = (cornerIndex == 0 || cornerIndex == 3) ? +cornerOffset.y : -cornerOffset.y;
                position = cornerOrigins[cornerIndex] + new Vector3(x, y, 0);
            }

            return RectTransformUtility.WorldToScreenPoint(null, position);
        }

        public void Initialize(string text, bool autoUpdate)
        {
            this.autoUpdate = autoUpdate;
            runtimeText = text;
        }
        public virtual void Show(Vector2? position)
        {
            if (isOpened)
            {
                return;
            }

            ManagerCoreUI.Instance.ShowTooltip(baseText + runtimeText, position ?? GetPosition());
            isOpened = true;
        }
        public virtual void Hide()
        {
            if (!isOpened)
            {
                return;
            }

            ManagerCoreUI.Instance.HideTooltip();
            isOpened = false;
        }
    }
}