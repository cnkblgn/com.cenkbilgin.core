using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public class UIPointerEventCursor : UIPointerEvent
    {
        [Header("_")]
        [SerializeField, Required] private string id = "default";

        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            ManagerCoreUI.Instance.SetCursor(id);
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            base.OnPointerExitInternal(eventData);

            ManagerCoreUI.Instance.SetCursor();
        }
    }
}
