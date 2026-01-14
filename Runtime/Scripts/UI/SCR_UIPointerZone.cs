using UnityEngine;
using UnityEngine.UI;
using Core;

namespace Core.UI
{
    using static CoreUtility;

    /// <summary>
    /// Creates empty quad for event detection, generates no image/draw call!
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIPointerZone : MaskableGraphic
    {
        public UIPointerZone() => useLegacyMeshGeneration = false;
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                return;
            }

            Gizmos.color = COLOR_RED;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
        }
#endif
    }
}