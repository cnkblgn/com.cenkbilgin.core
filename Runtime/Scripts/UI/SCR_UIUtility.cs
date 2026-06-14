using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public static class UIUtility
    {
        public static void ScrollToView(this RectTransform thisTransform, ScrollRect scrollRect)
        {
            Rect rect = scrollRect.GetComponent<RectTransform>().rect;
            RectTransform content = scrollRect.content;

            // The position of the selected UI element is the absolute anchor position,
            // ie. the local position within the scroll rect + its height if we're
            // scrolling down. If we're scrolling up it's just the absolute anchor position.
            float selectedPositionY = Mathf.Abs(thisTransform.anchoredPosition.y) + thisTransform.rect.height;
            // The upper bound of the scroll view is the anchor position of the content we're scrolling.
            float scrollViewMinY = content.anchoredPosition.y;
            // The lower bound is the anchor position + the height of the scroll rect.
            float scrollViewMaxY = content.anchoredPosition.y + rect.height;
            // If the selected position is below the current lower bound of the scroll view we scroll down.
            if (selectedPositionY > scrollViewMaxY)
            {
                float newY = selectedPositionY - rect.height;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);
            }
            // If the selected position is above the current upper bound of the scroll view we scroll up.
            else if (Mathf.Abs(thisTransform.anchoredPosition.y) < scrollViewMinY)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Abs(thisTransform.anchoredPosition.y + thisTransform.sizeDelta.y / 2));
            }
        }
        public static void ClampToView(this RectTransform thisTransform, RectTransform rootCanvas)
        {
            ClampToView(thisTransform, rootCanvas, thisTransform.anchoredPosition);
        }
        public static void ClampToView(this RectTransform thisTransform, RectTransform rootCanvas, Vector2 screenPosition)
        {
            if (rootCanvas == null)
            {
                Debug.LogError("rootCanvas == null!");
                return;
            }

            Vector2 pivot = thisTransform.pivot;

            if (pivot.x > 0 || pivot.y > 0)
            {
                Debug.LogWarning("wrong pivot, pivot must be = (0,0)");
            }

            Vector2 position = screenPosition / rootCanvas.localScale.x;

            Rect rootRect = rootCanvas.rect;
            Rect thisRect = thisTransform.rect;

            if (position.x + thisRect.width > rootRect.width) position.x = rootRect.width - thisRect.width;
            if (position.y + thisRect.height > rootRect.height) position.y = rootRect.height - thisRect.height;

            thisTransform.anchoredPosition = position;
        }

        public static void BindSelectable(this Button thisButton, Selectable selectableUp, Selectable selectableDown, Selectable selectableLeft, Selectable selectableRight)
        {
            Navigation navigationData = thisButton.navigation;

            if (selectableUp != null)
            {
                navigationData.selectOnUp = selectableUp;
            }

            if (selectableDown != null)
            {
                navigationData.selectOnDown = selectableDown;
            }

            if (selectableLeft != null)
            {
                navigationData.selectOnLeft = selectableLeft;
            }

            if (selectableRight != null)
            {
                navigationData.selectOnRight = selectableRight;
            }

            thisButton.navigation = navigationData;
        }

        public static void AlignTopLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new(0, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignTopCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignTopRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignMiddleRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomLeft(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomCenter(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignBottomRight(this RectTransform rectTransform, Vector2 offset = default)
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = offset;
        }
        public static void AlignStretch(this RectTransform rectTransform, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            rectTransform.pivot = new(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        public static void Hide(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 0 && !canvasGroup.interactable && !canvasGroup.blocksRaycasts)
            {
                return;
            }

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        public static void Show(this CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 1 && canvasGroup.interactable && canvasGroup.blocksRaycasts)
            {
                return;
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        public static void Show(this CanvasGroup canvasGroup, bool isInteractable, bool isBlockRaycast)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (canvasGroup.alpha == 1 && canvasGroup.interactable == isInteractable && canvasGroup.blocksRaycasts == isBlockRaycast)
            {
                return;
            }

            canvasGroup.alpha = 1;
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isBlockRaycast;
        }
        public static void Hide(this Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            if (!canvas.enabled)
            {
                return;
            }

            canvas.enabled = false;
        }
        public static void Show(this Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            if (canvas.enabled)
            {
                return;
            }

            canvas.enabled = true;
        }

        /// <summary> It Does not cause rebuild mesh </summary> ///
        public static void SetCanvasColor(this Image image, Color color) => image.canvasRenderer.SetColor(color);
        /// <summary> It Does not cause rebuild mesh </summary> ///
        public static float GetCanvasAlpha(this Image image) => image.canvasRenderer.GetAlpha();
        public static void SetCanvasAlpha(this Image image, float alpha) => image.canvasRenderer.SetAlpha(alpha);



        public static void Schedule(this TaskInstanceTweenFadeCanvas task, float start, float target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenFillImage task, float start, float target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenFadeImage task, Color startColor, Color targetColor, float startAlpha, float targetAlpha)
        {
            task.Reset();

            task.OverrideStart(startColor, startAlpha);
            task.OverrideTarget(targetColor, targetAlpha);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenBlinkImage task, Color start, Color target, float interval)
        {
            task.Reset();

            task.OverrideInterval(interval);
            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenOffsetLayout task, RectOffset start, RectOffset target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenOffsetRect task, Vector2 start, Vector2 target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenOffsetRectX task, Vector2 start, Vector2 target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }
        public static void Schedule(this TaskInstanceTweenOffsetRectY task, Vector2 start, Vector2 target)
        {
            task.Reset();

            task.OverrideStart(start);
            task.OverrideTarget(target);

            TaskSystem.TryCreate(task);
        }

        public static TaskInstanceTweenOffsetLayout Offset(this LayoutGroup layoutGroup, RectOffset start, RectOffset target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetLayout obj = new(layoutGroup, start, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenOffsetRect Offset(this RectTransform rectTransform, Vector2 start, Vector2 target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRect obj = new(rectTransform, start, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenOffsetRectX OffsetX(this RectTransform rectTransform, float start, float target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRectX obj = new(rectTransform, start, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenOffsetRectY OffsetY(this RectTransform rectTransform, float start, float target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenOffsetRectY obj = new(rectTransform, start, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenFadeCanvas Fade(this CanvasGroup canvasGroup, float target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFadeCanvas obj = new(canvasGroup, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenFadeImage Fade(this Image image, Color targetColor, float targetAlpha, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFadeImage obj = new(image, targetColor, targetAlpha, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenBlinkImage Blink(this Image image, Color start, Color target, float interval, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenBlinkImage obj = new(image, start, target, interval, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
        public static TaskInstanceTweenFillImage Fill(this Image image, float target, float fadeSeconds, float waitSeconds = 0, TweenType tweenType = TweenType.UNSCALED, EaseType easeType = EaseType.LINEAR, Action onComplete = null)
        {
            TaskInstanceTweenFillImage obj = new(image, target, fadeSeconds, waitSeconds, tweenType, easeType, onComplete);
            TaskSystem.TryCreate(obj);

            return obj;
        }
    }
}
