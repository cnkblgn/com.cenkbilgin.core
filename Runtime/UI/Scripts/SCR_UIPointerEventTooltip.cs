using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UIPointerEventTooltip : UIPointerEvent
    {
        public bool IsFocused { get; private set; } = false;

        private enum Corner : int
        {
            BOTTOM_LEFT = 0,
            TOP_LEFT = 1,
            TOP_RIGHT = 2,
            BOTTOM_RIGHT = 3,
            CENTER = 4,
        }

        [Header("_")]
        [Info("Optional")]
        [SerializeField] private UITooltipView targetTooltip = null;

        [Header("_")]
        [SerializeField] private bool disableEventPosition = true;
        [SerializeField] private Corner cornerIndex = Corner.TOP_LEFT;
        [SerializeField] private Vector2 cornerOffset = new(0, 0);

        [Header("_")]
        [SerializeField, TextArea()] private string baseText = STRING_EMPTY;

        private RectTransform thisTransform = null;
        private readonly Vector3[] cornerOrigins = new Vector3[4];
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

            ManagerUI.Instance.MoveTooltip(eventData.position);
        }
        private Vector2 GetPosition()
        {
            Vector3 position;

            if (cornerIndex == Corner.CENTER)
            {
                position = thisTransform.TransformPoint(thisTransform.rect.center);
            }
            else
            {
                thisTransform.GetWorldCorners(cornerOrigins);

                bool isLeft = cornerIndex == Corner.TOP_LEFT || cornerIndex == Corner.BOTTOM_LEFT;
                bool isTop = cornerIndex == Corner.TOP_LEFT || cornerIndex == Corner.TOP_RIGHT;
                float x = isLeft ? +cornerOffset.x : -cornerOffset.x;
                float y = isTop ? -cornerOffset.y : +cornerOffset.y;

                position = cornerOrigins[(int)cornerIndex] + new Vector3(x, y, 0);
            }

            return RectTransformUtility.WorldToScreenPoint(null, position);
        }

        public void Initialize(string text, bool autoUpdate)
        {
            this.autoUpdate = autoUpdate;
            runtimeText = text;
        }
        public void Show(Vector2? position)
        {
            if (isOpened)
            {
                return;
            }

            Vector2 screenPosition = position ?? GetPosition();

            if (targetTooltip != null)
            {
                targetTooltip.Show(baseText + runtimeText, screenPosition);
            }
            else
            {
                ManagerUI.Instance.ShowTooltip(baseText + runtimeText, screenPosition);
            }

            isOpened = true;
        }
        public void Hide()
        {
            if (!isOpened)
            {
                return;
            }

            if (targetTooltip != null)
            {
                targetTooltip.Hide();
            }
            else
            {
                ManagerUI.Instance.HideTooltip();
            }

            isOpened = false;
        }
    }
}