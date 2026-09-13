using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    using static CoreUtility;

    public sealed class UIPointerEventDrag : UIPointerEvent
    {
        [Header("_")]
        [SerializeField, Required] private Canvas targetCanvas;

        [Header("_")]
        [SerializeField] private bool clampToCanvas = true;

        private RectTransform thisTransform = null;
        private RectTransform canvasTransform = null;
        private readonly Vector3[] thisCorners = new Vector3[4];
        private readonly Vector3[] canvasCorners = new Vector3[4];
        private Vector2 canvasSize = Vector2.zero;
        private Vector2 windowSize = Vector2.zero;

        private void Awake()
        {
            thisTransform = GetComponent<RectTransform>();
            canvasTransform = targetCanvas.GetComponent<RectTransform>();

            canvasSize = canvasTransform.rect.size;
            windowSize = thisTransform.rect.size;
        }

        protected override void OnBeginDragInternal(PointerEventData eventData)
        {
            base.OnBeginDragInternal(eventData);

            thisTransform.SetAsLastSibling();
        }
        protected override void OnEndDragInternal(PointerEventData eventData)
        {
            base.OnEndDragInternal(eventData);
        }
        protected override void OnDragInternal(PointerEventData eventData)
        {
            base.OnDragInternal(eventData);

            thisTransform.anchoredPosition += eventData.delta / targetCanvas.scaleFactor;

            if (clampToCanvas)
            {
                ClampToCanvas();
            }
        }

        private void ClampToCanvas()
        {
            Vector2 half = windowSize * 0.5f;
            Vector2 pos = thisTransform.anchoredPosition;

            float minX = -canvasSize.x * 0.5f + half.x;
            float maxX = canvasSize.x * 0.5f - half.x;

            float minY = -canvasSize.y * 0.5f + half.y;
            float maxY = canvasSize.y * 0.5f - half.y;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            thisTransform.anchoredPosition = pos;
        }
    }
}
