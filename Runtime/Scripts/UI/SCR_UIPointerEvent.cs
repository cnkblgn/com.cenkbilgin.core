using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIPointerEvent : MonoBehaviour, IDeselectHandler, ISubmitHandler, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public event Action<PointerEventData> PointerOnClick = null;
        public event Action<PointerEventData> PointerOnEnter = null;
        public event Action<PointerEventData> PointerOnExit = null;
        public event Action<BaseEventData> GamepadOnSubmit = null;
        public event Action<BaseEventData> GamepadOnSelect = null;
        public event Action<BaseEventData> GamepadOnDeselect = null;

        public void OnPointerClick(PointerEventData eventData) => OnPointerClickInternal(eventData);
        protected virtual void OnPointerClickInternal(PointerEventData eventData) => PointerOnClick?.Invoke(eventData);
        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterInternal(eventData);
        protected virtual void OnPointerEnterInternal(PointerEventData eventData) => PointerOnEnter?.Invoke(eventData);
        public void OnPointerExit(PointerEventData eventData) => OnPointerExitInternal(eventData);
        protected virtual void OnPointerExitInternal(PointerEventData eventData) => PointerOnExit?.Invoke(eventData);
        public void OnSubmit(BaseEventData eventData) => OnSubmitInternal(eventData);
        protected virtual void OnSubmitInternal(BaseEventData eventData) => GamepadOnSubmit?.Invoke(eventData);
        public void OnSelect(BaseEventData eventData) => OnSelectInternal(eventData);
        protected virtual void OnSelectInternal(BaseEventData eventData) => GamepadOnSelect?.Invoke(eventData);
        public void OnDeselect(BaseEventData eventData) => OnDeselectInternal(eventData);
        protected virtual void OnDeselectInternal(BaseEventData eventData) => GamepadOnDeselect?.Invoke(eventData);
        public void OnBeginDrag(PointerEventData eventData) => OnBeginDragInternal(eventData);
        protected virtual void OnBeginDragInternal(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) => OnEndDragInternal(eventData);
        protected virtual void OnEndDragInternal(PointerEventData eventData) { }
        public void OnDrag(PointerEventData eventData) => OnDragInternal(eventData);
        protected virtual void OnDragInternal(PointerEventData eventData) { }
    }
}