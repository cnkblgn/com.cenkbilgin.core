using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    public class UIPointerEventExecuteParent : UIPointerEvent
    {
        [Header("_")]
        [SerializeField] private GameObject targetObject = null;

        protected override void OnSubmitInternal(BaseEventData eventData)
        {
            base.OnSubmitInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.submitHandler);
        }
        protected override void OnSelectInternal(BaseEventData eventData)
        {
            base.OnSelectInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.selectHandler);
        }
        protected override void OnDeselectInternal(BaseEventData eventData)
        {
            base.OnDeselectInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.deselectHandler);
        }
        protected override void OnPointerClickInternal(PointerEventData eventData)
        {
            base.OnPointerClickInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
        }
        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
        }
        protected override void OnPointerExitInternal(PointerEventData eventData)
        {
            base.OnPointerExitInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.pointerExitHandler);
        }
        protected override void OnBeginDragInternal(PointerEventData eventData)
        {
            base.OnBeginDragInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
        protected override void OnEndDragInternal(PointerEventData eventData)
        {
            base.OnEndDragInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
        protected override void OnDragInternal(PointerEventData eventData)
        {
            base.OnDragInternal(eventData);

            ExecuteEvents.ExecuteHierarchy(targetObject != null ? targetObject : transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }
}
