using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class UIPointerEventCursor : UIPointerEvent
    {
        [Header("_")]
        [SerializeField] private UICursorType type = UICursorType.CLICK;

        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            switch (type)
            {
                case UICursorType.DEFAULT:
                    break;
                case UICursorType.GRAB:
                    ManagerCoreUI.Instance.SetCursor(UICursorType.GRAB);
                    break;
                case UICursorType.CLICK:
                    ManagerCoreUI.Instance.SetCursor(UICursorType.CLICK);
                    break;
                case UICursorType.INPUT:
                    ManagerCoreUI.Instance.SetCursor(UICursorType.INPUT);
                    break;
                default:
                    break;
            }
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            base.OnPointerExitInternal(eventData);

            ManagerCoreUI.Instance.SetCursor(UICursorType.DEFAULT);
        }
    }
}
