using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIPointerEvent : MonoBehaviour, IDeselectHandler, ISubmitHandler, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerMoveHandler
    {
        public event Action<PointerEventData> PointerOnClick = null;
        public event Action<PointerEventData> PointerOnEnter = null;
        public event Action<PointerEventData> PointerOnExit = null;
        public event Action<PointerEventData> PointerOnUp = null;
        public event Action<PointerEventData> PointerOnDown = null;
        public event Action<PointerEventData> PointerOnDragBegin = null;
        public event Action<PointerEventData> PointerOnDragEnd = null;
        public event Action<PointerEventData> PointerOnDrag = null;
        public event Action<PointerEventData> PointerOnMove = null;
        public event Action<BaseEventData> GamepadOnSubmit = null;
        public event Action<BaseEventData> GamepadOnSelect = null;
        public event Action<BaseEventData> GamepadOnDeselect = null;

        public void OnPointerClick(PointerEventData eventData) => OnPointerClickInternal(eventData);
        protected virtual void OnPointerClickInternal(PointerEventData eventData) => PointerOnClick?.Invoke(eventData);
        public void OnPointerEnter(PointerEventData eventData) => OnPointerEnterInternal(eventData);
        protected virtual void OnPointerEnterInternal(PointerEventData eventData) => PointerOnEnter?.Invoke(eventData);
        public void OnPointerExit(PointerEventData eventData) => OnPointerExitInternal(eventData);
        protected virtual void OnPointerExitInternal(PointerEventData eventData) => PointerOnExit?.Invoke(eventData);
        public void OnPointerMove(PointerEventData eventData) => OnPointerMoveInternal(eventData);
        protected virtual void OnPointerMoveInternal(PointerEventData eventData) => PointerOnMove?.Invoke(eventData);
        public void OnPointerUp(PointerEventData eventData) => OnPointerUpInternal(eventData);
        public virtual void OnPointerUpInternal(PointerEventData eventData) => PointerOnUp?.Invoke(eventData);
        public void OnPointerDown(PointerEventData eventData) => OnPointerDownInternal(eventData);
        public virtual void OnPointerDownInternal(PointerEventData eventData) => PointerOnDown?.Invoke(eventData);
        public void OnBeginDrag(PointerEventData eventData) => OnBeginDragInternal(eventData);
        protected virtual void OnBeginDragInternal(PointerEventData eventData) => PointerOnDragBegin?.Invoke(eventData);
        public void OnEndDrag(PointerEventData eventData) => OnEndDragInternal(eventData);
        protected virtual void OnEndDragInternal(PointerEventData eventData) => PointerOnDragEnd?.Invoke(eventData);
        public void OnDrag(PointerEventData eventData) => OnDragInternal(eventData);
        protected virtual void OnDragInternal(PointerEventData eventData) => PointerOnDrag?.Invoke(eventData);
        public void OnSubmit(BaseEventData eventData) => OnSubmitInternal(eventData);
        protected virtual void OnSubmitInternal(BaseEventData eventData) => GamepadOnSubmit?.Invoke(eventData);
        public void OnSelect(BaseEventData eventData) => OnSelectInternal(eventData);
        protected virtual void OnSelectInternal(BaseEventData eventData) => GamepadOnSelect?.Invoke(eventData);
        public void OnDeselect(BaseEventData eventData) => OnDeselectInternal(eventData);
        protected virtual void OnDeselectInternal(BaseEventData eventData) => GamepadOnDeselect?.Invoke(eventData);
    }
}