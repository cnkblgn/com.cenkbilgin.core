using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public static class TweenUtility
    {
        public static TweenInstanceOffsetLayout Offset(this LayoutGroup layoutGroup, RectOffset startValue, RectOffset targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceOffsetLayout tweenObject = new(layoutGroup, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceOffsetRect Offset(this RectTransform rectTransform, Vector2 startValue, Vector2 targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceOffsetRect tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceOffsetRectX OffsetX(this RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceOffsetRectX tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceOffsetRectY OffsetY(this RectTransform rectTransform, float startValue, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceOffsetRectY tweenObject = new(rectTransform, startValue, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceFadeCanvas Fade(this CanvasGroup canvasGroup, float targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceFadeCanvas tweenObject = new(canvasGroup, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceFadeImage Fade(this Image image, Color targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceFadeImage tweenObject = new(image, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
        public static TweenInstanceScaleTransform Scale(this Transform transform, Vector3 targetValue, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TweenInstanceScaleTransform tweenObject = new(transform, targetValue, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TweenSystem.TryCreate(tweenObject);

            return tweenObject;
        }
    }
}
