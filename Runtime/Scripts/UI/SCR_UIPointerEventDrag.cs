using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.UI
{
    using static CoreUtility;

    public class UIPointerEventDrag : UIPointerEvent
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

        [Obsolete]
        private void ClampToCanvasOld()
        {
            thisTransform.GetWorldCorners(thisCorners);
            canvasTransform.GetWorldCorners(canvasCorners);

            Vector3 offset = Vector3.zero;

            // LEFT
            if (thisCorners[0].x < canvasCorners[0].x)
            {
                offset.x = canvasCorners[0].x - thisCorners[0].x;
            }

            // RIGHT
            if (thisCorners[2].x > canvasCorners[2].x)
            {
                offset.x = canvasCorners[2].x - thisCorners[2].x;
            }

            // BOTTOM
            if (thisCorners[0].y < canvasCorners[0].y)
            {
                offset.y = canvasCorners[0].y - thisCorners[0].y;
            }

            // TOP
            if (thisCorners[1].y > canvasCorners[1].y)
            {
                offset.y = canvasCorners[1].y - thisCorners[1].y;
            }

            if (offset != Vector3.zero)
            {
                // apply offset in world space
                thisTransform.position += offset;
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
