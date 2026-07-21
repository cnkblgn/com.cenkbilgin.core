using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    internal sealed class UIViewportCanvas
    {
        public readonly Camera Camera = null;
        public readonly Canvas Canvas = null;
        public readonly RectTransform Transform = null;
        public readonly GraphicRaycaster Raycaster = null;

        public UIViewportCanvas(Camera camera, Canvas canvas, RectTransform transform, GraphicRaycaster raycaster)
        {
            Camera = camera;

            Canvas = canvas;
            Transform = transform;
            Raycaster = raycaster;

            Canvas.renderMode = RenderMode.WorldSpace;
            Canvas.worldCamera = Camera;
            Canvas.worldCamera.allowHDR = false;
            Canvas.worldCamera.allowMSAA = false;
            Canvas.worldCamera.depthTextureMode = DepthTextureMode.None;

            if (Raycaster != null)
            {
                Raycaster.enabled = false;
            }
        }
    }
}
