using UnityEngine;
using Core.Graphics;

namespace Core.Misc
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIAspectRatioFitter : MonoBehaviour
    {
        private RectTransform thisTransform = null;
        private readonly float thisAspect = 16f / 9f;

        private void OnEnable()
        {
            thisTransform = GetComponent<RectTransform>();
            ManagerCoreGraphics.OnResolutionChanged += OnResolutionChanged;
        }
        private void OnDisable()
        {
            ManagerCoreGraphics.OnResolutionChanged -= OnResolutionChanged;
        }
        private void OnResolutionChanged(Int2 resolution)
        {
            float currentAspect = (float)resolution.x / (float)resolution.y;

            if (Application.isEditor)
            {
                currentAspect = (float)Screen.width / (float)Screen.height;
            }

            if (currentAspect >= thisAspect)
            {
                float scale = thisAspect / currentAspect;
                thisTransform.anchorMin = new Vector2(0.5f - 0.5f * scale, 0);
                thisTransform.anchorMax = new Vector2(0.5f + 0.5f * scale, 1);
            }
            else
            {
                thisTransform.anchorMin = new Vector2(0, 0);
                thisTransform.anchorMax = new Vector2(1, 1);
            }

            thisTransform.offsetMin = Vector2.zero;
            thisTransform.offsetMax = Vector2.zero;
        }
    }
}